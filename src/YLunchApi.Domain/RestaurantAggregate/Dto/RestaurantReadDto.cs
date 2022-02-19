using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Dto;

public class RestaurantReadDto : EntityReadDto
{
    public string Name { get; set; } = null!;
    public string AdminId { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsEmailConfirmed { get; set; }
    public DateTime? EmailConfirmationDateTime { get; set; }
    public bool IsOpen { get; set; }
    public bool IsCurrentlyOpenToOrder { get; set; }
    public bool IsCurrentlyOpenInPlace { get; set; }
    public bool IsPublic { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime? LastUpdateDateTime { get; set; }

    // address
    public string ZipCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string City { get; set; } = null!;
    public string StreetNumber { get; set; } = null!;
    public string StreetName { get; set; } = null!;

    public string? AddressExtraInformation { get; set; }
    // !address

    public ICollection<ClosingDateReadDto> ClosingDates { get; set; } = new List<ClosingDateReadDto>();
    public ICollection<PlaceOpeningTimeReadDto> PlaceOpeningTimes { get; set; } = new List<PlaceOpeningTimeReadDto>();
    public ICollection<OrderOpeningTimeReadDto> OrderOpeningTimes { get; set; } = new List<OrderOpeningTimeReadDto>();

    public string? Base64Image { get; set; }
    public string? Base64Logo { get; set; }
}
