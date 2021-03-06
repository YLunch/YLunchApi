using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IProductRepository _productRepository;

    public OrderRepository(ApplicationDbContext context, IDateTimeProvider dateTimeProvider, IProductRepository productRepository)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _productRepository = productRepository;
    }

    public async Task CreateOrder(Order order)
    {
        await _context.Orders.AddAsync(order);
        var soldOutProductIds = new SortedSet<string>();
        foreach (var orderedProduct in order.OrderedProducts)
        {
            var product = await _productRepository.GetProductById(orderedProduct.ProductId);
            product.Quantity = product.Quantity switch
            {
                0 => AddToSoldOut(ref soldOutProductIds, product),
                > 0 => product.Quantity.Value - 1,
                _ => null
            };
        }

        if (soldOutProductIds.Count > 0)
        {
            throw new SoldOutProductsException(string.Join(",", soldOutProductIds));
        }

        await _context.SaveChangesAsync();
    }

    private static int AddToSoldOut(ref SortedSet<string> soldOutProductIds, Product product)
    {
        soldOutProductIds.Add(product.Id);
        return product.Quantity!.Value;
    }

    public async Task<Order> GetOrderById(string orderId)
    {
        var order = await OrdersQueryBase
            .FirstOrDefaultAsync(x => x.Id == orderId);
        if (order == null)
        {
            throw new EntityNotFoundException();
        }

        return order;
    }

    public async Task<ICollection<Order>> GetOrders(OrderFilter orderFilter)
    {
        var query = FilterByRestaurantId(OrdersQueryBase, orderFilter.RestaurantId);
        query = FilterByCustomerId(query, orderFilter.CustomerId);
        query = FilterByDate(query, orderFilter.MinCreationDateTime, orderFilter.MaxCreationDateTime);
        query = FilterByCurrentOrderState(query, orderFilter.OrderStates);

        var orders = await query
                           .Skip((orderFilter.Page - 1) * orderFilter.Size)
                           .Take(orderFilter.Size)
                           .ToListAsync();
        return FormatOrders(orders);
    }

    public async Task<ICollection<Order>> AddStatusToOrders(string restaurantId, SortedSet<string> orderIds, OrderState orderState)
    {
        var orders = await OrdersQueryBase
                           .Where(x => x.RestaurantId == restaurantId)
                           .Where(x => orderIds.Contains(x.Id)).ToListAsync();

        if (orders.Count < orderIds.Count)
        {
            var notFoundOrderIds = orderIds.Except(orders.Select(x => x.Id));
            throw new EntityNotFoundException($"Orders: {string.Join(" and ", notFoundOrderIds)} not found.");
        }

        foreach (var order in orders)
        {
            order.OrderStatuses.Add(new OrderStatus
            {
                OrderId = order.Id,
                DateTime = _dateTimeProvider.UtcNow,
                OrderState = orderState
            });
        }

        await _context.SaveChangesAsync();
        return FormatOrders(orders);
    }

    private IQueryable<Order> OrdersQueryBase =>
        _context.Orders
                .Include(x => x.OrderStatuses)
                .Include(x => x.OrderedProducts);

    private static IQueryable<Order> FilterByRestaurantId(IQueryable<Order> query, string? restaurantId) =>
        restaurantId switch
        {
            null => query,
            _ => query.Where(x => x.RestaurantId == restaurantId)
        };

    private static IQueryable<Order> FilterByCustomerId(IQueryable<Order> query, string? customerId) =>
        customerId switch
        {
            null => query,
            _ => query.Where(x => x.UserId == customerId)
        };

    private static IQueryable<Order> FilterByDate(IQueryable<Order> query, DateTime? minCreationDateTime, DateTime? maxCreationDateTime) =>
        (minDateTime: minCreationDateTime, maxDateTime: maxCreationDateTime) switch
        {
            (null, null) => query,
            ({ }, null) => query.Where(x => x.CreationDateTime.Date >= ((DateTime)minCreationDateTime).Date),
            (null, { }) => query.Where(x => x.CreationDateTime.Date <= ((DateTime)maxCreationDateTime).Date),
            ({ }, { }) => query.Where(x =>
                x.CreationDateTime.Date >= ((DateTime)minCreationDateTime).Date &&
                x.CreationDateTime.Date <= ((DateTime)maxCreationDateTime).Date)
        };

    private static IQueryable<Order> FilterByCurrentOrderState(IQueryable<Order> query, SortedSet<OrderState>? orderStates) =>
        orderStates switch
        {
            null => query,
            _ => orderStates
                .Aggregate(query, (acc, x) => acc
                    .Where(o => x == o.OrderStatuses.OrderBy(y => y.DateTime).Last().OrderState))
        };

    private static Order FormatOrder(Order order)
    {
        order.OrderStatuses = order.OrderStatuses.OrderBy(x => x.OrderState).ToList();
        order.OrderedProducts = order.OrderedProducts.OrderBy(x => x.ProductType).ToList();
        return order;
    }

    private static ICollection<Order> FormatOrders(IEnumerable<Order> orders) =>
        orders.Select(FormatOrder)
              .OrderBy(x => x.CreationDateTime)
              .ThenBy(x => x.CurrentOrderStatus.DateTime)
              .ToList();
}
