using YLunchApi.Domain.CommonAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public class ProductTag : Entity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } =
        new List<Product>();
}
