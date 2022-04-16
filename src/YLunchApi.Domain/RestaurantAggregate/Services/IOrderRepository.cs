using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IOrderRepository
{
    Task CreateOrder(Order order);
    Task<Order> GetOrderById(string orderId);
    Task<ICollection<Order>> GetOrders(OrderFilter orderFilter);
    Task<ICollection<Order>> AddStatusToOrders(BulkOrderStatusCreateDto bulkOrderStatusCreateDto);
}
