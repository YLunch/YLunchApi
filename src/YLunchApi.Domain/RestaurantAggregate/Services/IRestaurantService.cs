using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IRestaurantService
{
    Task<RestaurantReadDto> Create(RestaurantCreateDto restaurantCreateDto, string restaurantAdminId);
    Task<RestaurantReadDto> GetById(string restaurantId);
    Task<ICollection<RestaurantReadDto>> GetRestaurants(RestaurantFilter restaurantFilter);
    bool IsOpenToOrder(Restaurant restaurant, DateTime dateTime);
    bool IsOpenInPlace(Restaurant restaurant, DateTime dateTime);
}
