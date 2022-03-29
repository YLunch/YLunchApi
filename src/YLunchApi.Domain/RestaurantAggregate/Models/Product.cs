using System.Diagnostics.CodeAnalysis;
using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public class Product : Entity
{
    public string RestaurantId { get; set; } = null!;

    [ExcludeFromCodeCoverage] public virtual Restaurant? Restaurant { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double Price { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreationDateTime { get; set; }
    public DateTime? ExpirationDateTime { get; set; }

    public ProductType ProductType { get; set; }

    public string? Image { get; set; }
    public int? Quantity { get; set; }

    public virtual ICollection<Allergen> Allergens { get; set; } =
        new List<Allergen>();

    public virtual ICollection<ProductTag> ProductTags { get; set; } =
        new List<ProductTag>();
}
