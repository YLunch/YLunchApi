using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.Core.Utils;

public static class OpeningTimeUtils
{
    public static int StartMinutesFromFirstDayOfWeek(OpeningTimeCreateDto openingTime)
    {
        return (int)openingTime.DayOfWeek * 24 * 60 + openingTime.OffsetOpenMinutes;
    }


    public static int EndMinutesFromFirstDayOfWeek(OpeningTimeCreateDto openingTime)
    {
        return StartMinutesFromFirstDayOfWeek(openingTime) + openingTime.OpenMinutes;
    }
}
