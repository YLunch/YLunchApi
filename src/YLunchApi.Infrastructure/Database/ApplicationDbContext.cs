using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.RestaurantAggregate;
using YLunchApi.Domain.UserAggregate;

namespace YLunchApi.Infrastructure.Database;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Allergen> Allergens { get; set; } = null!;
    public DbSet<ProductTag> ProductTags { get; set; } = null!;
    public DbSet<OrderedProduct> OrderedProducts { get; set; } = null!;
    public DbSet<ClosingDate> ClosingDates { get; set; } = null!;
    public DbSet<OpeningTime> OpeningTimes { get; set; } = null!;
    public DbSet<Restaurant> Restaurants { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
}
