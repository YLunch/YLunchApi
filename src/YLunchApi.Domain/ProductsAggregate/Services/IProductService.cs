using YLunchApi.Domain.ProductsAggregate.Dto;

namespace YLunchApi.Domain.ProductsAggregate.Services;

public interface IProductService
{ 
    Task<ProductReadDto> Create(ProductCreateDto productCreateDto, string restaurantId);
}