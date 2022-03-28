using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IProductRepository
{
    Task Create(Product product);
    Task<Product> GetById(string productId);
}
