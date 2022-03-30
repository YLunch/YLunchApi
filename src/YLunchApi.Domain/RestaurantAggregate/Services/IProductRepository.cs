using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IProductRepository
{
    Task Create(Product product);
    Task<Product> GetById(string productId);
    Task<ICollection<Product>> GetProducts(ProductFilter productFilter);
}
