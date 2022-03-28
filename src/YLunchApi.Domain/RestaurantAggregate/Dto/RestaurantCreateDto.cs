using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class RestaurantCreateDto
{
    [Required] public string Name { get; set; } = null!;

    [Required]
    [RegularExpression(
        @"^0[6-7][0-9]{8}$",
        ErrorMessage = "PhoneNumber is invalid. Example: '0612345678'.")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [RegularExpression(
        @"^[a-z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)*\.[a-z]{2,20}$",
        ErrorMessage = "Email is invalid. It should be lowercase email format. Example: example@example.com.")]
    public string Email { get; set; } = null!;

    [Required] public bool IsOpen { get; set; }

    [Required] public bool IsPublic { get; set; }

    // address
    [Required]
    [RegularExpression("[0-9]{0,5}", ErrorMessage = "ZipCode is invalid. Example: '06560'.")]
    public string ZipCode { get; set; } = null!;

    [Required] public string Country { get; set; } = null!;
    [Required] public string City { get; set; } = null!;
    [Required] public string StreetNumber { get; set; } = null!;
    [Required] public string StreetName { get; set; } = null!;

    public string AddressExtraInformation { get; set; } = "";
    // !address

    public ICollection<ClosingDateCreateDto> ClosingDates { get; set; } = new List<ClosingDateCreateDto>();

    [NonOverridingOpeningTimes]
    public ICollection<OpeningTimeCreateDto> PlaceOpeningTimes { get; set; } =
        new List<OpeningTimeCreateDto>();

    [NonOverridingOpeningTimes]
    public ICollection<OpeningTimeCreateDto> OrderOpeningTimes { get; set; } =
        new List<OpeningTimeCreateDto>();

    public string? Base64Image { get; set; }
    public string? Base64Logo { get; set; }
}
