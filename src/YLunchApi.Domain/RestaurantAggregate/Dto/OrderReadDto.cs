using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OrderReadDto : EntityReadDto
{
    public string UserId { get; set; } = null!;
    public string RestaurantId { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime ReservedForDateTime { get; set; }
    public DateTime CreationDateTime { get; set; }
    public double TotalPrice { get; set; }

    public string? CustomerComment { get; set; }
    public string? RestaurantComment { get; set; }
    public DateTime? AcceptationDateTime { get; set; }
    public bool IsAccepted => AcceptationDateTime != null;

    public ICollection<OrderStatusReadDto> OrderStatuses { get; set; } = null!;

    public OrderStatusReadDto CurrentOrderStatus { get; set; } = null!;

    public bool IsAcknowledged { get; set; }

    public ICollection<OrderedProductReadDto> OrderedProducts { get; set; } = null!;
}
