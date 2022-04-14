using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OrderUpdateDto
{
    [Required] public OrderState? OrderState { get; set; }
    public string? RestaurantComment { get; set; }
}
