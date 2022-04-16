using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OrderRepository(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task CreateOrder(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
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
        var query = OrdersQueryBase
                    .Skip((orderFilter.Page - 1) * orderFilter.Size)
                    .Take(orderFilter.Size);
        query = FilterByRestaurantId(query, orderFilter.RestaurantId);
        query = FilterByCurrentOrderState(query, orderFilter.OrderStates);

        var orders = await query.ToListAsync();
        return orders.Select(FormatOrder)
                     .OrderBy(x => x.CreationDateTime)
                     .ToList();
    }

    public async Task<ICollection<Order>> AddStatusToOrders(BulkOrderStatusCreateDto bulkOrderStatusCreateDto)
    {
        var orders = await OrdersQueryBase
                           .Where(x => bulkOrderStatusCreateDto.OrderIds!.Contains(x.Id)).ToListAsync();

        if (orders.Count < bulkOrderStatusCreateDto.OrderIds!.Count)
        {
            throw new EntityNotFoundException("Order");
        }

        foreach (var order in orders)
        {
            order.OrderStatuses.Add(new OrderStatus
            {
                OrderId = order.Id,
                DateTime = _dateTimeProvider.UtcNow,
                State = (OrderState)bulkOrderStatusCreateDto.OrderState!
            });
        }

        await _context.SaveChangesAsync();
        return orders;
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

    private static IQueryable<Order> FilterByCurrentOrderState(IQueryable<Order> query, SortedSet<OrderState>? orderStates) =>
        orderStates switch
        {
            null => query,
            _ => orderStates
                .Aggregate(query, (acc, x) => acc
                    .Where(o => x == o.OrderStatuses.Last().State))
        };

    private static Order FormatOrder(Order order)
    {
        order.OrderStatuses = order.OrderStatuses.OrderBy(x => x.State).ToList();
        order.OrderedProducts = order.OrderedProducts.OrderBy(x => x.ProductType).ToList();
        return order;
    }
}
