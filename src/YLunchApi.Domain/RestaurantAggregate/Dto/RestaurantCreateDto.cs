using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class RestaurantCreateDto
{
    [Required] public string Name { get; set; } = null!;

    [Required]
    [RegularExpression(
        @"^0[6-7][0-9]{8}$",
        ErrorMessage = "PhoneNumber is not allowed. Example: 0612345678.")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [RegularExpression(
        @"^[a-z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)*\.[a-z]{2,20}$",
        ErrorMessage = "Email is invalid. Should use right format with no uppercase.")]
    public string Email { get; set; } = null!;

    [Required] public bool IsOpen { get; set; }

    [Required] public bool IsPublic { get; set; }

    // address
    [Required] public string ZipCode { get; set; } = null!;
    [Required] public string Country { get; set; } = null!;
    [Required] public string City { get; set; } = null!;
    [Required] public string StreetNumber { get; set; } = null!;
    [Required] public string StreetName { get; set; } = null!;

    public string AddressExtraInformation { get; set; } = "";
    // !address

    [Required] public ICollection<ClosingDateCreateDto> ClosingDates { get; set; } = null!;

    [Required] public ICollection<OpeningTimeCreateDto> OpeningTimes { get; set; } = null!;

    public string? Base64Image { get; set; }
    public string? Base64Logo { get; set; }
}
