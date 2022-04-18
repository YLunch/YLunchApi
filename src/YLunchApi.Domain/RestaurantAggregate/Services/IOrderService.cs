using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IOrderService
{
    Task<OrderReadDto> CreateOrder(string customerId, string restaurantId, OrderCreateDto orderCreateDto);
    Task<OrderReadDto> GetOrderByIdForCustomer(string customerId, string orderId);
    Task<OrderReadDto> GetOrderByIdForRestaurantAdmin(string restaurantAdminId, string orderId);
    Task<ICollection<OrderReadDto>> GetOrders(OrderFilter orderFilter);
    Task<ICollection<OrderReadDto>> AddStatusToOrders(string restaurantId, AddStatusToOrdersDto addStatusToOrdersDto);
}
