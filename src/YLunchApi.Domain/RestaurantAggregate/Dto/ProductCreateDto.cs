using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class ProductCreateDto
{
    [Required] [Lowercase] public string Name { get; set; } = null!;
    [Lowercase] public string? Description { get; set; }
    [Required] public double? Price { get; set; }
    [Required] public bool? IsActive { get; set; }
    [Range(1, 10000)] public int? Quantity { get; set; }
    [Required] public ProductType? ProductType { get; set; }
    [InFutureOrNull] public DateTime? ExpirationDateTime { get; set; }
    public string? Image { get; set; }
    [Required] public ICollection<AllergenCreateDto> Allergens { get; set; } = null!;

    public ICollection<ProductTagCreateDto> ProductTags { get; set; } =
        new List<ProductTagCreateDto>();
}
