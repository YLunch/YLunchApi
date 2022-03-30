using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Filters;

public class RestaurantFilter
{
    [Range(1, 30, ErrorMessage = "Size must be an integer between 1 and 30.")]
    public int Size { get; set; } = 30;

    [Range(1, 100000, ErrorMessage = "Page must be an integer between 1 and 100000.")]
    public int Page { get; set; } = 1;

    public bool? IsCurrentlyOpenToOrder { get; set; }
    public bool? IsPublished { get; set; }
}
