using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RestaurantRepository(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Create(Restaurant restaurant)
    {
        var existingRestaurant = await _context.Restaurants.FirstOrDefaultAsync(x =>
            x.Name == restaurant.Name &&
            x.Country == restaurant.Country &&
            x.City == restaurant.City &&
            x.ZipCode == restaurant.ZipCode &&
            x.StreetName == restaurant.StreetName &&
            x.StreetNumber == restaurant.StreetNumber);
        if (existingRestaurant != null)
        {
            throw new EntityAlreadyExistsException();
        }

        restaurant.ClosingDates = restaurant.ClosingDates.Select(x =>
        {
            var existingClosingDate =
                _context.ClosingDates.FirstOrDefault(
                    closingDateDb => closingDateDb.ClosingDateTime == x.ClosingDateTime);
            return existingClosingDate ?? x;
        }).ToList();

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();
    }

    public async Task<Restaurant> GetById(string id)
    {
        var restaurant = await RestaurantsQueryBase
            .FirstOrDefaultAsync(x => x.Id == id);
        if (restaurant == null) throw new EntityNotFoundException($"Restaurant {id} not found.");
        return ReformatRestaurant(restaurant);
    }

    public async Task<ICollection<Restaurant>> GetRestaurants(RestaurantFilter restaurantFilter)
    {
        var query = FilterByRestaurantAdminId(RestaurantsQueryBase, restaurantFilter.RestaurantAdminId);
        query = FilterByIsPublished(query, restaurantFilter.IsPublished);
        query = FilterByIsCurrentlyOpenToOrder(query, restaurantFilter.IsCurrentlyOpenToOrder);
        query = query.Skip((restaurantFilter.Page - 1) * restaurantFilter.Size)
                     .Take(restaurantFilter.Size);
        return (await query.ToListAsync()).Select(ReformatRestaurant).ToList();
    }

    private IOrderedQueryable<Restaurant> RestaurantsQueryBase =>
        _context.Restaurants
                .Include(x => x.ClosingDates.OrderBy(y => y.ClosingDateTime))
                .Include(x => x.PlaceOpeningTimes.OrderBy(y => (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes))
                .Include(x => x.OrderOpeningTimes.OrderBy(y => (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes))
                .OrderBy(restaurant => restaurant.CreationDateTime);

    private static IQueryable<Restaurant> FilterByIsPublished(IQueryable<Restaurant> query, bool? isPublished) =>
        isPublished switch
        {
            true => query.Where(x => x.IsPublished),
            false => query.Where(x => !x.IsPublished),
            null => query
        };

    private IQueryable<Restaurant> FilterByIsCurrentlyOpenToOrder(IQueryable<Restaurant> query, bool? isCurrentlyOpenToOrderRestaurants)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        return isCurrentlyOpenToOrderRestaurants switch
        {
            true => query.Where(x => x.OrderOpeningTimes.Any(y =>
                (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes <= (int)utcNow.DayOfWeek * 1440 + utcNow.Hour * 60 + utcNow.Minute &&
                (int)utcNow.DayOfWeek * 1440 + utcNow.Hour * 60 + utcNow.Minute <= (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes + y.DurationInMinutes)),

            false => query.Where(x => !x.OrderOpeningTimes.Any(y =>
                (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes <= (int)utcNow.DayOfWeek * 1440 + utcNow.Hour * 60 + utcNow.Minute &&
                (int)utcNow.DayOfWeek * 1440 + utcNow.Hour * 60 + utcNow.Minute <= (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes + y.DurationInMinutes)),

            null => query
        };
    }

    private static IQueryable<Restaurant> FilterByRestaurantAdminId(IQueryable<Restaurant> query, string? restaurantAdminId) =>
        restaurantAdminId switch
        {
            null => query,
            not null => query.Where(x => x.AdminId == restaurantAdminId)
        };

    private static Restaurant ReformatRestaurant(Restaurant restaurant)
    {
        restaurant.ClosingDates = restaurant.ClosingDates.OrderBy(x => x.ClosingDateTime).ToList();
        restaurant.PlaceOpeningTimes = OpeningTimeUtils.AscendingOrder(restaurant.PlaceOpeningTimes);
        restaurant.OrderOpeningTimes = OpeningTimeUtils.AscendingOrder(restaurant.OrderOpeningTimes);
        return restaurant;
    }
}
