using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class ProductTagCreateDto
{
    [Required] public string Name { get; set; } = null!;
}