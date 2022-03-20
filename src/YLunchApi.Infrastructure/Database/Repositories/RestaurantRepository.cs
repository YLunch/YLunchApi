using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly ApplicationDbContext _context;

    public RestaurantRepository(ApplicationDbContext context)
    {
        _context = context;
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
        if (restaurant == null) throw new EntityNotFoundException($"Restaurant {id} not found");
        return ReorderRestaurantFields(restaurant);
    }

    public async Task<ICollection<Restaurant>> GetRestaurants(RestaurantFilter restaurantFilter)
    {
        var query = RestaurantsQueryBase
                    .Skip((restaurantFilter.Page - 1) * restaurantFilter.Size)
                    .Take(restaurantFilter.Size);
        query = IsPublishedRestaurantsQuery(query, restaurantFilter.IsPublished);
        query = IsCurrentlyOpenToOrderRestaurantsQuery(query, restaurantFilter.IsCurrentlyOpenToOrder);
        return (await query.ToListAsync()).Select(ReorderRestaurantFields).ToList();
    }

    private IOrderedQueryable<Restaurant> RestaurantsQueryBase =>
        _context.Restaurants
                .Include(x => x.ClosingDates.OrderBy(y => y.ClosingDateTime))
                .Include(x => x.PlaceOpeningTimes.OrderBy(y => (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes))
                .Include(x => x.OrderOpeningTimes.OrderBy(y => (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes))
                .OrderBy(restaurant => restaurant.CreationDateTime);

    private static IQueryable<Restaurant> IsPublishedRestaurantsQuery(IQueryable<Restaurant> query, bool? isPublished) =>
        isPublished switch
        {
            true => query.Where(x => x.IsPublished),
            false => query.Where(x => !x.IsPublished),
            null => query
        };

    private static IQueryable<Restaurant> IsCurrentlyOpenToOrderRestaurantsQuery(IQueryable<Restaurant> query, bool? isCurrentlyOpenToOrderRestaurants) =>
        isCurrentlyOpenToOrderRestaurants switch
        {
            true => query.Where(x => x.OrderOpeningTimes.Any(y =>
                (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes <= (int)DateTime.UtcNow.DayOfWeek * 1440 + DateTime.UtcNow.Hour * 60 + DateTime.UtcNow.Minute &&
                (int)DateTime.UtcNow.DayOfWeek * 1440 + DateTime.UtcNow.Hour * 60 + DateTime.UtcNow.Minute <= (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes + y.DurationInMinutes)),

            false => query.Where(x => !x.OrderOpeningTimes.Any(y =>
                (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes <= (int)DateTime.UtcNow.DayOfWeek * 1440 + DateTime.UtcNow.Hour * 60 + DateTime.UtcNow.Minute &&
                (int)DateTime.UtcNow.DayOfWeek * 1440 + DateTime.UtcNow.Hour * 60 + DateTime.UtcNow.Minute <= (int)y.DayOfWeek * 24 * 60 + y.OffsetInMinutes + y.DurationInMinutes)),

            null => query
        };

    private static Restaurant ReorderRestaurantFields(Restaurant restaurant)
    {
        restaurant.ClosingDates = restaurant.ClosingDates.OrderBy(x=>x.ClosingDateTime).ToList();
        restaurant.PlaceOpeningTimes = OpeningTimeUtils.AscendingOrder(restaurant.PlaceOpeningTimes);
        restaurant.OrderOpeningTimes = OpeningTimeUtils.AscendingOrder(restaurant.OrderOpeningTimes);
        return restaurant;
    }
}
