using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class AllergenReadDto : EntityReadDto
{
    public string Name { get; set; } = null!;
}
