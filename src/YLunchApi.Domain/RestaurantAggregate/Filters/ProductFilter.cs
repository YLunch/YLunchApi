using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Filters;

public class ProductFilter
{
    [Range(1, 250, ErrorMessage = "Size must be an integer between 1 and 250.")]
    public int Size { get; set; } = 250;

    [Range(1, 100000, ErrorMessage = "Page must be an integer between 1 and 100000.")]
    public int Page { get; set; } = 1;

    public string? RestaurantId { get; set; }

    public bool? IsAvailable { get; set; }

    public SortedSet<string>? ProductIds { get; set; }
}
