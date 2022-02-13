using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IRestaurantService
{
    Task<RestaurantReadDto> Create(RestaurantCreateDto restaurantCreateDto, string restaurantAdminId);
    Task<RestaurantReadDto> GetById(string restaurantId);
}
