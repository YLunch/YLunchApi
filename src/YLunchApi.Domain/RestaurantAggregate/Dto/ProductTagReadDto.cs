using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class ProductTagReadDto : EntityReadDto
{
    public string Name { get; set; } = null!;
}
