using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Exceptions;
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
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
        if (order == null)
        {
            throw new EntityNotFoundException();
        }

        return order;
    }
}
