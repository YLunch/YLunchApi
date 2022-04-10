using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IProductService
{
    Task<ProductReadDto> Create(ProductCreateDto productCreateDto, string restaurantId);
    Task<ProductReadDto> GetById(string productId);
    Task<ICollection<ProductReadDto>> GetProducts(ProductFilter productFilter);
}
