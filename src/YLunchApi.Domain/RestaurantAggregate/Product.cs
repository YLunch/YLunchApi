using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.CommonAggregate;
using YLunchApi.Domain.RestaurantAggregate.Enums;

namespace YLunchApi.Domain.RestaurantAggregate;

public class Product : Entity
{
    public string RestaurantId { get; set; } = null!;
    public virtual Restaurant? Restaurant { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
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
