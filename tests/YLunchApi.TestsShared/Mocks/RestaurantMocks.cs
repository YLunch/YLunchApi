using Mapster;
using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.TestsShared.Mocks;

public static class RestaurantMocks
{
    public static readonly RestaurantCreateDto RestaurantCreateDto = new()
    {
        Email = "admin@restaurant.com",
        PhoneNumber = "0612345678",
        Name = "My restaurant",
        IsOpen = true,
        IsPublic = true,
        City = "Valbonne",
        Country = "France",
        StreetName = "Place Sophie Lafitte",
        ZipCode = "06560",
        StreetNumber = "1"
    };

    public static RestaurantReadDto RestaurantReadDto(string id)
    {
        var restaurantReadDto = RestaurantCreateDto.Adapt<RestaurantReadDto>();
        restaurantReadDto.Id = id;
        restaurantReadDto.IsCurrentlyOpenToOrder = restaurantReadDto.IsOpen &&
                                                   // Todo set also based on order limit time
                                                   !restaurantReadDto.ClosingDates.Any(x =>
                                                       x.ClosingDateTime.Date.Equals(DateTime.UtcNow.Date));
        return restaurantReadDto;
    }
}
