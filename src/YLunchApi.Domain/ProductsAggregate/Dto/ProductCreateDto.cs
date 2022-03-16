using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.ProductsAggregate.Dto;
using System.ComponentModel.DataAnnotations;

public class ProductCreateDto
{
    [Required] public string Name { get; set; } = null!;
    public string ? Description { get; set; }
    [Required] public double Price { get; set; }
    [Required] public bool IsActive { get; set; }
    [Required] public ProductType ProductType { get; set; }

    public string? Image { get; set; }
    public int? Quantity { get; set; }

    [Required] public ICollection<AllergenCreateDto> Allergens { get; set; } = null!;

    public ICollection<ProductTag> ProductTags { get; set; } =
        new List<ProductTag>();
}