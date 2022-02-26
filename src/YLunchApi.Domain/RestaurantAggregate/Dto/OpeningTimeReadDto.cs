using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OpeningTimeReadDto : EntityReadDto
{
    public string RestaurantId { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public int OffsetInMinutes { get; set; }
    public int DurationInMinutes { get; set; }
}
