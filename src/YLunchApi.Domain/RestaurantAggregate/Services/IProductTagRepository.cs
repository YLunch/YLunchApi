using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IProductTagRepository
{
    Task<ProductTag> GetByName(string name);
}
