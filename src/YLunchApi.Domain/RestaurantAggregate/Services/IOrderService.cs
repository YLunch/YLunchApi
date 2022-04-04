using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IOrderService
{
    Task<OrderReadDto> Create(string restaurantId, OrderCreateDto orderCreateDto);
}
