using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OpeningTimeCreateDto
{
    [Required]
    [Range(0, 6, ErrorMessage = "Day must be in range 0-6, 0 is sunday, 6 is saturday.")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    [Range(0, 1439, ErrorMessage = "OffsetOpenMinutes should be less than number of minutes in a day.")]
    public int OffsetOpenMinutes { get; set; }

    [Required]
    [Range(0, 7 * 24 * 60 - 1, ErrorMessage = "OpenMinutes should be less than number of minutes in a week.")]
    public int OpenMinutes { get; set; }
}
