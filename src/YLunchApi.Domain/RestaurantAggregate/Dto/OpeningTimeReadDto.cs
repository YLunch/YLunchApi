using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OpeningTimeReadDto : EntityReadDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public int StartTimeInMinutes { get; set; }
    public int EndTimeInMinutes { get; set; }
    public int? StartOrderTimeInMinutes { get; set; }
    public int? EndOrderTimeInMinutes { get; set; }
}
