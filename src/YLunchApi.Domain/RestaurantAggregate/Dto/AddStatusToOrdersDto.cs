using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class AddStatusToOrdersDto
{
    [Required] [ListOfId] public SortedSet<string>? OrderIds { get; set; }

    [Required]
    [Range(0, OrderStateUtils.Count - 1)]
    public OrderState? OrderState { get; set; }
}
