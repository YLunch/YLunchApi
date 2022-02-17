using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class OpeningTimeCreateDto
{
    [Required]
    [Range(0, 6, ErrorMessage = "Day must be in range 0-6, 0 is sunday, 6 is saturday")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    [Range(0, 1439, ErrorMessage = "Minutes from midnight should be in range of 0 and 1439 (23h59)")]
    public int StartTimeInMinutes { get; set; }

    [Required]
    [Range(0, 1439, ErrorMessage = "Minutes from midnight should be in range of 0 and 1439 (23h59)")]
    public int EndTimeInMinutes { get; set; }

    [Range(0, 1439, ErrorMessage = "Minutes from midnight should be in range of 0 and 1439 (23h59)")]
    public int? StartOrderTimeInMinutes { get; set; }

    [Range(0, 1439, ErrorMessage = "Minutes from midnight should be in range of 0 and 1439 (23h59)")]
    public int? EndOrderTimeInMinutes { get; set; }
}
