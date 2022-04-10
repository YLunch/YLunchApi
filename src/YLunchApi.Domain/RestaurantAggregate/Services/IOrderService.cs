using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IOrderService
{
    Task<OrderReadDto> Create(string customerId, string restaurantId, OrderCreateDto orderCreateDto);
    Task<OrderReadDto> GetByIdForCustomer(string customerId, string orderId);
    Task<OrderReadDto> GetByIdForRestaurantAdmin(string restaurantAdminId, string orderId);
}
