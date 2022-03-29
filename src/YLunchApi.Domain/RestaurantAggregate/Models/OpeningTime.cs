using System.Diagnostics.CodeAnalysis;
using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public abstract class OpeningTime : Entity
{
    public string RestaurantId { get; [ExcludeFromCodeCoverage] set; } = null!;

    [ExcludeFromCodeCoverage] public virtual Restaurant? Restaurant { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int OffsetInMinutes { get; set; }
    public int DurationInMinutes { get; set; }


    public bool Contains(DateTime dateTime)
    {
        var dateTimeMinutesFromFirstDayOfWeek =
            (dateTime.DayOfWeek < DayOfWeek ? 7 : 0 + (int)dateTime.DayOfWeek) * 24 * 60 +
            dateTime.Hour * 60 + dateTime.Minute;


        return dateTimeMinutesFromFirstDayOfWeek >= OpeningTimeUtils.StartMinutesFromFirstDayOfWeek(this) &&
               dateTimeMinutesFromFirstDayOfWeek <= OpeningTimeUtils.EndMinutesFromFirstDayOfWeek(this);
    }
}
