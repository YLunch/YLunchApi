using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IOrderService
{
    Task<OrderReadDto> Create(string customerId, string restaurantId, OrderCreateDto orderCreateDto);
    Task<OrderReadDto> GetByIdForCustomer(string customerId, string orderId);
    Task<OrderReadDto> GetByIdForRestaurantAdmin(string restaurantAdminId, string orderId);
    Task<ICollection<OrderReadDto>> GetOrders(OrderFilter orderFilter);
}
