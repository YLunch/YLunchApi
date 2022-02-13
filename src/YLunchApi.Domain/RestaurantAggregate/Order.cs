using System.ComponentModel.DataAnnotations;
using YLunch.DomainShared.RestaurantAggregate.Enums;
using YLunchApi.Domain.CommonAggregate;
using YLunchApi.Domain.UserAggregate;

namespace YLunchApi.Domain.RestaurantAggregate;

public class Order : Entity
{
    public string UserId { get; set; } = null!;
    public virtual User? Customer { get; set; }

    public string RestaurantId { get; set; } = null!;
    public virtual Restaurant? Restaurant { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime ReservedForDateTime { get; set; }
    public DateTime CreationDateTime { get; set; }
    public double TotalPrice { get; set; }

    public string? CustomerComment { get; set; }
    public string? RestaurantComment { get; set; }
    public DateTime? AcceptationDateTime { get; set; }
    public bool IsAccepted => AcceptationDateTime != null;

    public virtual ICollection<OrderStatus> OrderStatuses { get; set; } =
        new List<OrderStatus>();

    public OrderStatus CurrentOrderStatus => OrderStatuses
        .OrderBy(x => x.DateTime)
        .Last();

    public bool IsAcknowledged =>
        OrderStatuses.Any(os => os.State.Equals(OrderState.Acknowledged));

    public virtual ICollection<OrderedProduct> OrderedProducts { get; set; } =
        new List<OrderedProduct>();
}
