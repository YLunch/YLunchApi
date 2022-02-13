using YLunchApi.Domain.CommonAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public class ClosingDate : Entity
{
    public DateTime ClosingDateTime { get; set; }

    public virtual ICollection<Restaurant> Restaurants { get; set; } =
        new List<Restaurant>();
}
