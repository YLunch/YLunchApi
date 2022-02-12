using YLunchApi.Domain.CommonAggregate;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OpeningTimeReadDto : EntityReadDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string StartTimeInMinutes { get; set; } = null!;
    public string EndTimeInMinutes { get; set; } = null!;
    public string? StartOrderTimeInMinutes { get; set; }
    public string? EndOrderTimeInMinutes { get; set; }
}
