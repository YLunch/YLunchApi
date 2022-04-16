using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IProductService
{
    Task<ProductReadDto> CreateProduct(ProductCreateDto productCreateDto, string restaurantId);
    Task<ProductReadDto> GetProductById(string productId);
    Task<ICollection<ProductReadDto>> GetProducts(ProductFilter productFilter);
}
