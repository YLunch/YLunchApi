using System.ComponentModel.DataAnnotations;
using YLunch.DomainShared.RestaurantAggregate.Enums;
using YLunchApi.Domain.CommonAggregate;

namespace YLunchApi.Domain.RestaurantAggregate;

public class OrderStatus : Entity
{
    public string OrderId { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public OrderState State { get; set; }
    public DateTime DateTime { get; set; }
}
