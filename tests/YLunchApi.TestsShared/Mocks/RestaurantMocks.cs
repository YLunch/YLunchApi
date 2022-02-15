using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.TestsShared.Mocks;

public static class RestaurantMocks
{
    public static RestaurantCreateDto RestaurantCreateDto => new()
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
}
