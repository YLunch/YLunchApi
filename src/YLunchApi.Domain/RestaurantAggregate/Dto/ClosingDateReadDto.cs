using YLunchApi.Domain.CommonAggregate;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class ClosingDateReadDto : EntityReadDto
{
    public DateTime ClosingDateTime { get; set; }
    public string RestaurantId { get; set; } = null!;
}
