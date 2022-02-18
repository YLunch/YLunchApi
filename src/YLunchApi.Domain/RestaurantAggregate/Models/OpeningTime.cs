using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Helpers.Extensions;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public class OpeningTime : Entity
{
    public DayOfWeek DayOfWeek { get; set; }
    public int OffsetOpenMinutes { get; set; }
    public int OpenMinutes { get; set; }
    public int OrderingOffsetOpenMinutes { get; set; }
    public int OrderingOpenMinutes { get; set; }

    public bool Contains(DateTime dateTime)
    {
        // Computation of difference between days
        var dateTimeMinutesToCompare =
            (dateTime.DayOfWeek < DayOfWeek ? 7 : 0 + dateTime.DayOfWeek - DayOfWeek) * 24 * 60 +
            dateTime.MinutesFromMidnight();

        return dateTimeMinutesToCompare >= OrderingOffsetOpenMinutes &&
               dateTimeMinutesToCompare <= OrderingOffsetOpenMinutes + OrderingOpenMinutes;
    }
}
