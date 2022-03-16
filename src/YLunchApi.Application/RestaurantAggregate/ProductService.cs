using YLunchApi.Domain.ProductsAggregate.Dto;
using YLunchApi.Domain.ProductsAggregate.Services;
using YLunchApi.Domain.RestaurantAggregate.Models;

namespace YLunchApi.Application.ProductAggregate;

public class ProductService
{
    private readonly IProductService _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductReadDto> Create(ProductCreateDto productCreateDto, string restaurantAdminId)
    {
        var restaurantId = Guid.NewGuid().ToString();
        var creationDateTime = DateTime.UtcNow;

        var restaurant = restaurantCreateDto.Adapt<Restaurant>();
        restaurant.Id = restaurantId;
        restaurant.AdminId = restaurantAdminId;
        restaurant.CreationDateTime = creationDateTime;
        restaurant.IsPublished = Restaurant.CanPublish(restaurant);

        await _productRepository.Create(product);
        var productDb = await _productRepository.GetById(productId);
        return productDb.Adapt<productReadDto>();
    }

    public async Task<ProductReadDto> GetById(string restaurantId)
    {
        var restaurant = await _productRepository.GetById(restaurantId);
        return restaurant.Adapt<productReadDto>();
    }
}