using Mapster;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Application.RestaurantAggregate;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RestaurantService(IRestaurantRepository restaurantRepository, IDateTimeProvider dateTimeProvider)
    {
        _restaurantRepository = restaurantRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    private RestaurantReadDto CreateRestaurantReadDto(Restaurant restaurant)
    {
        var restaurantReadDto = restaurant.Adapt<RestaurantReadDto>();
        restaurantReadDto.IsPublished = CanPublish(restaurant);
        restaurantReadDto.IsCurrentlyOpenToOrder = IsCurrentlyOpenToOrder(restaurant);
        restaurantReadDto.IsCurrentlyOpenInPlace = IsCurrentlyOpenInPlace(restaurant);
        return restaurantReadDto;
    }

    private bool IsCurrentlyOpenInPlace(Restaurant restaurant)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        return restaurant.IsOpen &&
               restaurant.ClosingDates.All(x => x.ClosingDateTime.Date != utcNow.Date) &&
               restaurant.PlaceOpeningTimes.Any(x => x.Contains(utcNow));
    }

    private bool IsCurrentlyOpenToOrder(Restaurant restaurant)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        return restaurant.IsOpen &&
               restaurant.ClosingDates.All(x => x.ClosingDateTime.Date != utcNow.Date) &&
               restaurant.OrderOpeningTimes.Any(x => x.Contains(utcNow));
    }

    private static bool CanPublish(Restaurant restaurant)
    {
        return restaurant.IsPublic &&
               !string.IsNullOrEmpty(restaurant.Name) &&
               !string.IsNullOrEmpty(restaurant.PhoneNumber) &&
               !string.IsNullOrEmpty(restaurant.Email) &&
               !string.IsNullOrEmpty(restaurant.ZipCode) &&
               !string.IsNullOrEmpty(restaurant.Country) &&
               !string.IsNullOrEmpty(restaurant.City) &&
               !string.IsNullOrEmpty(restaurant.StreetNumber) &&
               !string.IsNullOrEmpty(restaurant.StreetName) &&
               !string.IsNullOrEmpty(restaurant.AdminId) &&
               (restaurant.PlaceOpeningTimes.Count > 0 || restaurant.OrderOpeningTimes.Count > 0);
        // Todo uncomment when create product is implemented
        // && Products.Any(x => x.IsActive); //NOSONAR
    }

    public async Task<RestaurantReadDto> Create(RestaurantCreateDto restaurantCreateDto, string restaurantAdminId)
    {
        var restaurant = restaurantCreateDto.Adapt<Restaurant>();
        restaurant.AdminId = restaurantAdminId;
        restaurant.CreationDateTime = _dateTimeProvider.UtcNow;
        restaurant.IsPublished = CanPublish(restaurant);

        await _restaurantRepository.Create(restaurant);
        var restaurantDb = await _restaurantRepository.GetById(restaurant.Id);
        return CreateRestaurantReadDto(restaurantDb);
    }

    public async Task<RestaurantReadDto> GetById(string restaurantId)
    {
        var restaurant = await _restaurantRepository.GetById(restaurantId);
        return CreateRestaurantReadDto(restaurant);
    }

    public async Task<ICollection<RestaurantReadDto>> GetRestaurants(RestaurantFilter restaurantFilter)
    {
        var restaurants = await _restaurantRepository.GetRestaurants(restaurantFilter);
        return restaurants.Select(CreateRestaurantReadDto).ToList();
    }
}
