using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OrderCreateDto
{
    [Required] [ListOfId] public ICollection<string>? ProductIds { get; set; }
    [Required] [Future] public DateTime? ReservedForDateTime { get; set; }
    public string? CustomerComment { get; set; }
}
