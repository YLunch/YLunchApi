using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class ProductTagCreateDto
{
    [Required] [Lowercase] public string Name { get; set; } = null!;
}
