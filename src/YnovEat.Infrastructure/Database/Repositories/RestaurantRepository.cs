using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using YnovEat.Domain.ModelsAggregate.RestaurantAggregate;
using YnovEat.Domain.ModelsAggregate.UserAggregate;
using YnovEat.Domain.Services.Database.Repositories;

namespace YnovEat.Infrastructure.Database.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly ApplicationDbContext _context;

        public RestaurantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Restaurant> GetById(string id) =>
            await QueryEnrichedRestaurant
                .FirstOrDefaultAsync(x => x.Id.Equals(id));

        private IIncludableQueryable<Restaurant, ICollection<OpeningTime>> QueryEnrichedRestaurant =>
            _context.Restaurants
                .Include(x => x.RestaurantProducts)
                .Include(x => x.ClosingDates)
                .Include(x => x.RestaurantUsers)
                .Include(x => x.Categories)
                .Include(x => x.WeekOpeningTimes.OrderBy(y => y.DayOfWeek))
                .ThenInclude(x => x.OpeningTimes);

        public async Task<Restaurant> GetByUserId(string id) =>
            await QueryEnrichedRestaurant
                .FirstOrDefaultAsync(x => x.RestaurantUsers
                    .Any(y => y.UserId.Equals(id))
                );

        public async Task AddAdmin(User user)
        {
            await _context.RestaurantUsers.AddAsync(new RestaurantOwner
            {
                UserId = user.Id,
                User = user
            });
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<RestaurantCategory>> GetAllRestaurantCategories()
        {
            return await _context.RestaurantCategories.ToListAsync();
        }

        public async Task<Restaurant> CreateRestaurant(Restaurant restaurant)
        {
            var owner = await _context.RestaurantUsers.FirstAsync(x => x.User.Id.Equals(restaurant.OwnerId));
            owner.RestaurantId = restaurant.Id;
            await _context.Restaurants.AddAsync(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        public async Task<Restaurant> UpdateRestaurant(Restaurant restaurant)
        {
            var uselessCategories =
                await _context.RestaurantCategories
                    .Where(x => x.Restaurants.Count == 0).ToListAsync();
            _context.RemoveRange(uselessCategories);

            await _context.SaveChangesAsync();
            return restaurant;
        }
    }
}
