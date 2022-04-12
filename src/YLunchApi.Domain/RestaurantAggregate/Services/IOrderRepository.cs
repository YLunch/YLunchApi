using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IOrderRepository
{
    Task Create(Order order);
    Task<Order> GetById(string orderId);
    Task<ICollection<Order>> GetOrders(OrderFilter orderFilter);
}
