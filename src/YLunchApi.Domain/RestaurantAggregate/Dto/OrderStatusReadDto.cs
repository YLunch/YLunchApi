using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OrderStatusReadDto : EntityReadDto
{
    public string OrderId { get; set; } = null!;
    public OrderState State { get; set; }
    public DateTime DateTime { get; set; }
}
