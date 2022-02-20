using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.Core.Utils;
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
        // Computation of difference during all the week
        var dateTimeMinutesInput =
            (dateTime.DayOfWeek < DayOfWeek ? 7 : 0 + dateTime.DayOfWeek - DayOfWeek) * 24 * 60 +
            dateTime.MinutesFromMidnight();

        return dateTimeMinutesInput >= OpeningTimeUtils.StartMinutesFromFirstDayOfWeek(this) &&
               dateTimeMinutesInput <= OpeningTimeUtils.EndMinutesFromFirstDayOfWeek(this);
    }
}
