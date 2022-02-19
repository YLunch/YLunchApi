using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Exceptions;
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
            x.Name.Equals(restaurant.Name) &&
            x.Country.Equals(restaurant.Country) &&
            x.City.Equals(restaurant.City) &&
            x.ZipCode.Equals(restaurant.ZipCode) &&
            x.StreetName.Equals(restaurant.StreetName) &&
            x.StreetNumber.Equals(restaurant.StreetNumber));
        if (existingRestaurant != null)
        {
            throw new EntityAlreadyExistsException();
        }

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();
    }

    public async Task<Restaurant> GetById(string id)
    {
        var restaurant = await _context.Restaurants
                                       .Include(x => x.ClosingDates)
                                       .Include(x => x.PlaceOpeningTimes)
                                       .Include(x => x.OrderOpeningTimes)
                                       .FirstOrDefaultAsync(x => x.Id.Equals(id));
        if (restaurant == null) throw new EntityNotFoundException($"Restaurant {id} not found");
        return restaurant;
    }
}
