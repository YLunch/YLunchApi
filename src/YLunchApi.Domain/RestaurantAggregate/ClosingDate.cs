using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.CommonAggregate;

namespace YLunchApi.Domain.RestaurantAggregate;

public class ClosingDate : Entity
{
    public DateTime ClosingDateTime { get; set; }

    public virtual ICollection<Restaurant> Restaurants { get; set; } =
        new List<Restaurant>();
}
