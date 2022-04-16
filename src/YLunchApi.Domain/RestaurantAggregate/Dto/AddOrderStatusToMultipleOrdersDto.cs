using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class AddOrderStatusToMultipleOrdersDto
{
    [Required] [ListOfId] public SortedSet<string>? OrderIds { get; set; }
    [Required] public OrderState? OrderState { get; set; }
}
