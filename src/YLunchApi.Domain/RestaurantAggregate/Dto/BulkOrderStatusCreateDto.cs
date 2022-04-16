using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class BulkOrderStatusCreateDto
{
    [Required] public string RestaurantId { get; set; } = null!;
    [Required] [ListOfId] public SortedSet<string>? OrderIds { get; set; }
    [Required] public OrderState? OrderState { get; set; }
    public string? RestaurantComment { get; set; }
}
