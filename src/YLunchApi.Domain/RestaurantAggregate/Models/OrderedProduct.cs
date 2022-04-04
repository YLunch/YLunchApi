using System.Diagnostics.CodeAnalysis;
using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

// Todo remove coverage exclusion
[ExcludeFromCodeCoverage]
public class OrderedProduct : Entity
{
    public string ProductId { get; set; } = null!;
    public virtual Product? Product { get; set; }

    public string RestaurantId { get; set; } = null!;
    public virtual Restaurant? Restaurant { get; set; }

    public string OrderId { get; set; } = null!;
    public virtual Order? Order { get; set; }

    public string UserId { get; set; } = null!;
    public virtual User? User { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public double Price { get; set; }
    public DateTime CreationDateTime { get; set; }

    public string Allergens { get; set; } = null!;
    public string ProductTags { get; set; } = null!;

    public string? Image { get; set; }
}
