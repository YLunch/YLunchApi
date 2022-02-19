using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public abstract class OpeningTimeReadDto : EntityReadDto
{
    public string RestaurantId { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public int OffsetOpenMinutes { get; set; }
    public int OpenMinutes { get; set; }
}
