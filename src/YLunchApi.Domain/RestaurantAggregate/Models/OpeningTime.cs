using System.Diagnostics.CodeAnalysis;
using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public abstract class OpeningTime : Entity
{
    public string RestaurantId { get; [ExcludeFromCodeCoverage] set; } = null!;

    [ExcludeFromCodeCoverage] public virtual Restaurant? Restaurant { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OffsetTime { get; set; }
    public int DurationInMinutes { get; set; }


    public bool Contains(DateTime dateTime)
    {
        var timeOnly = TimeOnly.FromDateTime(dateTime);
        return timeOnly >= OffsetTime &&
               timeOnly <= OffsetTime.AddMinutes(DurationInMinutes);
    }
}
