using Mapster;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Application.ProductAggregate;

public class ProductService: IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<ProductReadDto> Create(ProductCreateDto productCreateDto, string restaurantId)
    {
        var product = productCreateDto.Adapt<Product>();
        product.Id = Guid.NewGuid().ToString();
        product.RestaurantId = restaurantId;
        product.CreationDateTime = DateTime.UtcNow;
        
        await _productRepository.Create(product);
        return product.Adapt<ProductReadDto>();
    }
}