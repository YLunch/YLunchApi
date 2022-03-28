using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.Domain.RestaurantAggregate.Services;

public interface IProductService
{
    Task<ProductReadDto> Create(ProductCreateDto productCreateDto, string restaurantId);
    Task<ProductReadDto> GetById(string productId);
}
