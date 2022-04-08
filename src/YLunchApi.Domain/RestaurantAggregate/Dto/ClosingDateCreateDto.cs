using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

namespace YLunchApi.Domain.RestaurantAggregate.Dto
{
    public class ClosingDateCreateDto
    {
        [Required] [Future] public DateTime ClosingDateTime { get; set; }
    }
}
