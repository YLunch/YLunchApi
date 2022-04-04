using System.Diagnostics.CodeAnalysis;
using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

// Todo remove coverage exclusion
[ExcludeFromCodeCoverage]
public class Order : Entity
{
    public string UserId { get; set; } = null!;
    public virtual User? User { get; set; }

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
        OrderStatuses.Any(os => os.State == OrderState.Acknowledged);

    public virtual ICollection<OrderedProduct> OrderedProducts { get; set; } =
        new List<OrderedProduct>();
}
