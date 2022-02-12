using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto
{
    public class OpeningTimeCreationDto
    {
        [Required] public string StartTimeInMinutes { get; set; } = null!;
        [Required] public string EndTimeInMinutes { get; set; } = null!;
        [Required] public string? StartOrderTimeInMinutes { get; set; }
        [Required] public string? EndOrderTimeInMinutes { get; set; }
    }
}
