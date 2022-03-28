using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class ProductCreateDto
{
    [Required] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [Required] public double? Price { get; set; } = null!;
    [Required] public bool? IsActive { get; set; } = null!;
    [Required] public ProductType? ProductType { get; set; } = null!;

    // Todo valid ExpirationDateTime is in future if present
    public DateTime? ExpirationDateTime { get; set; }
    public string? Image { get; set; }
    public int? Quantity { get; set; }

    [Required] public ICollection<AllergenCreateDto> Allergens { get; set; } = null!;

    public ICollection<ProductTagCreateDto> ProductTags { get; set; } =
        new List<ProductTagCreateDto>();
}
