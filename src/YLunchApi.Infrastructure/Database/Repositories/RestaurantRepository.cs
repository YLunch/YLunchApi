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

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();
    }

    public async Task<Restaurant> GetById(string id)
    {
        var restaurant = await _context.Restaurants
                                       .Include(x => x.ClosingDates)
                                       .Include(x => x.PlaceOpeningTimes)
                                       .Include(x => x.OrderOpeningTimes)
                                       .FirstOrDefaultAsync(x => x.Id == id);
        if (restaurant == null) throw new EntityNotFoundException($"Restaurant {id} not found");
        return restaurant;
    }
}
