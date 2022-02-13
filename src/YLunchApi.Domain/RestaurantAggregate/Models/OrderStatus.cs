using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public class OrderStatus : Entity
{
    public string OrderId { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public OrderState State { get; set; }
    public DateTime DateTime { get; set; }
}
