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

    public bool IsCurrentlyOpenToOrder =>
        IsOpen &&
        // Todo set also based on order limit time
        !ClosingDates.Any(x => x.ClosingDateTime.Date.Equals(DateTime.UtcNow.Date));

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
    public virtual ICollection<OpeningTime> OpeningTimes { get; set; } = new List<OpeningTime>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public static bool CanPublish(Restaurant restaurant) => //NOSONAR
        restaurant.IsPublic && //NOSONAR
        !string.IsNullOrEmpty(restaurant.Name) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.PhoneNumber) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.Email) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.ZipCode) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.Country) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.City) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.StreetNumber) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.StreetName) && //NOSONAR
        !string.IsNullOrEmpty(restaurant.AdminId) && //NOSONAR
        restaurant.OpeningTimes.Count > 0; //NOSONAR
    // Todo uncomment when create product is implemented
    // && Products.Any(x => x.IsActive); //NOSONAR

    public bool IsPublished { get; set; }
}
