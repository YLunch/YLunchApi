using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class ProductReadDto : EntityReadDto
{
    public string RestaurantId { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreationDateTime { get; set; }
    public DateTime? ExpirationDateTime { get; set; }

    public ProductType ProductType { get; set; }
    public ICollection<AllergenReadDto> Allergens { get; set; } = null!;
    public ICollection<ProductTagReadDto> ProductTags { get; set; } = null!;
    public string? Image { get; set; }
    public int? Quantity { get; set; }
}
