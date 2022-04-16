using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IProductRepository
{
    Task CreateProduct(Product product);
    Task<Product> GetProductById(string productId);
    Task<ICollection<Product>> GetProducts(ProductFilter productFilter);
    IQueryable<Product> ProductsQueryBase { get; }
}
