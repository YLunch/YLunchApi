using YLunchApi.Domain.ProductsAggregate.Dto;
using YLunchApi.Domain.ProductsAggregate.Services;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Application.ProductAggregate;

public class ProductService: IProductService
{
    public Task<ProductReadDto> Create(ProductCreateDto productCreateDto, string restaurantId)
    {
        throw new NotImplementedException();
    }
}