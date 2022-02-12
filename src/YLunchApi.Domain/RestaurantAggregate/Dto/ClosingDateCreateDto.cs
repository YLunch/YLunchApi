using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto
{
    public class ClosingDateCreateDto
    {
        // Todo valid date if in the future
        [Required] public DateTime ClosingDateTime { get; set; }
    }
}
