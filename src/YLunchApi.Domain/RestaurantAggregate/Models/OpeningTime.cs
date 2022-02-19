using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Helpers.Extensions;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public abstract class OpeningTime : Entity
{
    public string RestaurantId { get; set; } = null!;
    public virtual Restaurant? Restaurant { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int OffsetOpenMinutes { get; set; }
    public int OpenMinutes { get; set; }

    public bool Contains(DateTime dateTime)
    {
        // Computation of difference between days
        var dateTimeMinutesToCompare =
            (dateTime.DayOfWeek < DayOfWeek ? 7 : 0 + dateTime.DayOfWeek - DayOfWeek) * 24 * 60 +
            dateTime.MinutesFromMidnight();

        return dateTimeMinutesToCompare >= OffsetOpenMinutes &&
               dateTimeMinutesToCompare <= OffsetOpenMinutes + OpenMinutes;
    }
}
