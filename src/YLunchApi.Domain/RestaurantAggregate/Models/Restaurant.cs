using YLunchApi.Domain.CommonAggregate.Models;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Models;

public class Restaurant : Entity
{
    public string AdminId { get; set; } = null!;
    public virtual User? Admin { get; set; }
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsOpen { get; set; }

    public bool IsEmailConfirmed { get; set; }
    public DateTime? EmailConfirmationDateTime { get; set; }

    public bool IsPublic { get; set; }
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

    public string? Base64Image { get; set; }
    public string? Base64Logo { get; set; }

    public virtual ICollection<ClosingDate> ClosingDates { get; set; } = new List<ClosingDate>();
    public virtual ICollection<PlaceOpeningTime> PlaceOpeningTimes { get; set; } = new List<PlaceOpeningTime>();
    public virtual ICollection<OrderOpeningTime> OrderOpeningTimes { get; set; } = new List<OrderOpeningTime>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public bool IsPublished { get; set; }
}
