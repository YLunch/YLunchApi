using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.CommonAggregate;

namespace YLunchApi.Domain.RestaurantAggregate;

public class ProductTag : Entity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } =
        new List<Product>();
}
