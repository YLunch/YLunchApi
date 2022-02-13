using System.Diagnostics.CodeAnalysis;
using YLunchApi.Domain.CommonAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

// Todo remove coverage exclusion
[ExcludeFromCodeCoverage]
public class Allergen : Entity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } =
        new List<Product>();
}
