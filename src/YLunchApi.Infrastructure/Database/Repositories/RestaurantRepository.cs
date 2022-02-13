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
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();
    }

    public async Task<Restaurant> GetById(string id)
    {
        var restaurant = await _context.Restaurants
                                       .Include(x => x.ClosingDates)
                                       .FirstOrDefaultAsync(x => x.Id.Equals(id));
        if (restaurant == null) throw new EntityNotFoundException($"Restaurant {id} not found");
        var closingDates = restaurant.ClosingDates;
        return restaurant;
    }
}
