using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto
{
    public class OpeningTimeCreateDto
    {
        [Required]
        [Range(0, 6, ErrorMessage = "Day must be in range 0-6, 0 is sunday, 6 is saturday")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required] public string StartTimeInMinutes { get; set; } = null!;
        [Required] public string EndTimeInMinutes { get; set; } = null!;
        [Required] public string? StartOrderTimeInMinutes { get; set; }
        [Required] public string? EndOrderTimeInMinutes { get; set; }
    }
}
