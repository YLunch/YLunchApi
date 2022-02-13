using Mapster;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Application.RestaurantAggregate;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantService(IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<RestaurantReadDto> Create(RestaurantCreateDto restaurantCreateDto, string restaurantAdminId)
    {
        var restaurantId = Guid.NewGuid().ToString();
        var creationDateTime = DateTime.UtcNow;

        var restaurant = restaurantCreateDto.Adapt<Restaurant>();
        restaurant.Id = restaurantId;
        restaurant.AdminId = restaurantAdminId;
        restaurant.CreationDateTime = creationDateTime;
        restaurant.ClosingDates = restaurantCreateDto.ClosingDates.Select(x =>
        {
            var closingDate = x.Adapt<ClosingDate>();
            closingDate.Restaurants.Add(restaurant);
            return closingDate;
        }).ToList();

        await _restaurantRepository.Create(restaurant);
        var restaurantDb = await _restaurantRepository.GetById(restaurantId);
        return restaurantDb.Adapt<RestaurantReadDto>();
    }

    public async Task<RestaurantReadDto> GetById(string restaurantId)
    {
        var restaurant = await _restaurantRepository.GetById(restaurantId);
        return restaurant.Adapt<RestaurantReadDto>();
    }
}
