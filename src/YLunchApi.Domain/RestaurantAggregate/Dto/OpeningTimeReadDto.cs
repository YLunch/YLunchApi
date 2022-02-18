using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OpeningTimeReadDto : EntityReadDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public int OffsetOpenMinutes { get; set; }
    public int OpenMinutes { get; set; }
    public int OrderingOffsetOpenMinutes { get; set; }
    public int OrderingOpenMinutes { get; set; }
}
