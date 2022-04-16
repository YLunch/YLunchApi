using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IRestaurantRepository
{
    Task CreateRestaurant(Restaurant restaurant);
    Task<Restaurant> GetRestaurantById(string id);
    Task<ICollection<Restaurant>> GetRestaurants(RestaurantFilter restaurantFilter);
}
