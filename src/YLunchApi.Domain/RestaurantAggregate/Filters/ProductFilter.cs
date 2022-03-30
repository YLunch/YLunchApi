using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Filters;

public class ProductFilter
{
    [Range(1, 50, ErrorMessage = "Size must be an integer between 1 and 50.")]
    public int Size { get; set; } = 50;

    [Range(1, 100000, ErrorMessage = "Page must be an integer between 1 and 100000.")]
    public int Page { get; set; } = 1;

    public string? RestaurantId { get; set; }

    public bool? IsAvailable { get; set; }
}
