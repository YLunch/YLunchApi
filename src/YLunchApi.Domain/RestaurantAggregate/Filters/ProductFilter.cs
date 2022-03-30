using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Filters;

public class ProductFilter
{
    [Range(1, 30, ErrorMessage = "PageSize must be an integer within 1 and 30")]

    public int PageSize { get; set; } = 30;

    [Range(1, 100000, ErrorMessage = "PageIndex must be an integer within 1 and 100000.")]

    public int Page { get; set; } = 1;

    public string? RestaurantId { get; set; }
}
