using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OrderedProductReadDto : EntityReadDto
{
    public string ProductId { get; set; } = null!;
    public string RestaurantId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string CustomerId { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public DateTime CreationDateTime { get; set; }

    public string Allergens { get; set; } = null!;
    public string ProductTags { get; set; } = null!;

    public string? Image { get; set; }
}
