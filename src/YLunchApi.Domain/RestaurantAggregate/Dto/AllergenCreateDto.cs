using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class AllergenCreateDto
{
    [Required] public string Name { get; set; } = null!;
}