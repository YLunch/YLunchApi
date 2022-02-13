using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IRestaurantRepository
{
    Task Create(Restaurant restaurant);
    Task<Restaurant> GetById(string id);
}
