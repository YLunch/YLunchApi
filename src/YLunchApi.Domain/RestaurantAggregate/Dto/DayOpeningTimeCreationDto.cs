using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto
{
    public class DayOpeningTimesCreationDto
    {
        [Required]
        [Range(0, 6, ErrorMessage = "Day must be in range 0-6, 0 is sunday, 6 is saturday")]
        public DayOfWeek DayOfWeek { get; set; }

        public ICollection<OpeningTimeCreationDto> OpeningTimes { get; set; } =
            new List<OpeningTimeCreationDto>();
    }
}
