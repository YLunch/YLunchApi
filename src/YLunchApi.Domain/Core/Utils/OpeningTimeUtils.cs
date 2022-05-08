using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.Core.Utils;

public static class OpeningTimeUtils
{
    // public static int StartMinutesFromFirstDayOfWeek(OpeningTime openingTime)
    // {
    //     return (int)openingTime.DayOfWeek * 24 * 60 + openingTime.OffsetTime;
    // }
    //
    // public static int StartMinutesFromFirstDayOfWeek(OpeningTimeCreateDto openingTime)
    // {
    //     return (int)openingTime.DayOfWeek! * 24 * 60 + (int)openingTime.OffsetTime!;
    // }
    //
    // public static int EndMinutesFromFirstDayOfWeek(OpeningTime openingTime)
    // {
    //     return StartMinutesFromFirstDayOfWeek(openingTime) + openingTime.DurationInMinutes;
    // }
    //
    // public static int EndMinutesFromFirstDayOfWeek(OpeningTimeCreateDto openingTime)
    // {
    //     return StartMinutesFromFirstDayOfWeek(openingTime) + (int)openingTime.DurationInMinutes!;
    // }

    public static IEnumerable<OpeningTimeCreateDto> AscendingOrder(IEnumerable<OpeningTimeCreateDto> openingTimes)
    {
        return openingTimes.OrderBy(x=>x.OffsetTime)
                           .ThenBy(x=>x.OffsetTime!.Value.AddMinutes(x.DurationInMinutes!.Value))
                           .ToList();
    }
}
