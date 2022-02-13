using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.CommonAggregate;

namespace YLunchApi.Domain.RestaurantAggregate;

public class OpeningTime : Entity
{
    public DayOfWeek DayOfWeek { get; set; }
    public int StartTimeInMinutes { get; set; }
    public int EndTimeInMinutes { get; set; }
    public int? StartOrderTimeInMinutes { get; set; }
    public int? EndOrderTimeInMinutes { get; set; }
}
