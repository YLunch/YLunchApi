using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Create(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task<Order> GetById(string orderId)
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

        var orders = await query.ToListAsync();
        return orders.Select(FormatOrder)
                     .OrderBy(x => x.CreationDateTime)
                     .ToList();
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

    private static Order FormatOrder(Order order)
    {
        order.OrderStatuses = order.OrderStatuses.OrderBy(x => x.State).ToList();
        order.OrderedProducts = order.OrderedProducts.OrderBy(x => x.ProductType).ToList();
        return order;
    }
}
