using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.Core.Utils;

public static class OpeningTimeUtils
{
    public static int StartMinutesInWeek(OpeningTimeCreateDto openingTime)
    {
        return (int)openingTime.DayOfWeek * 24 * 60 + openingTime.OffsetOpenMinutes;
    }


    public static int EndMinutesInWeek(OpeningTimeCreateDto openingTime)
    {
        return StartMinutesInWeek(openingTime) + openingTime.OpenMinutes;
    }
}
