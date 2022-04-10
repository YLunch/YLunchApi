using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.TestsShared.Mocks;

public static class RestaurantMocks
{
    public static RestaurantCreateDto SimpleRestaurantCreateDto => new()
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

    public static RestaurantCreateDto PrepareFullRestaurant(string restaurantName, DateTime dateTime)
    {
        var restaurantCreateDto = SimpleRestaurantCreateDto;
        restaurantCreateDto.Name = restaurantName;

        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = dateTime.AddDays(2) },
            new() { ClosingDateTime = dateTime.AddDays(1) }
        };

        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = dateTime.AddDays(-1).DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = dateTime.DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            }
        };

        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = dateTime.AddDays(-1).DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = dateTime.DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            }
        };
        return restaurantCreateDto;
    }
}
