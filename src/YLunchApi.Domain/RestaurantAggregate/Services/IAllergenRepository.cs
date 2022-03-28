using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IAllergenRepository
{
    Task<Allergen> GetByName(string name);
}
