using Mapster;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Application.RestaurantAggregate;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IAllergenRepository _allergenRepository;
    private readonly IProductTagRepository _productTagRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProductService(IDateTimeProvider dateTimeProvider,
                          IProductRepository productRepository,
                          IAllergenRepository allergenRepository,
                          IProductTagRepository productTagRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _productRepository = productRepository;
        _allergenRepository = allergenRepository;
        _productTagRepository = productTagRepository;
    }

    public async Task<ProductReadDto> CreateProduct(ProductCreateDto productCreateDto, string restaurantId)
    {
        var product = productCreateDto.Adapt<Product>();
        product.RestaurantId = restaurantId;
        product.CreationDateTime = _dateTimeProvider.UtcNow;

        product.Allergens = product.Allergens
                                   .Select(async x =>
                                   {
                                       try
                                       {
                                           return await _allergenRepository.GetAllergenByName(x.Name);
                                       }
                                       catch (EntityNotFoundException)
                                       {
                                           return x;
                                       }
                                   })
                                   .Select(t => t.Result)
                                   .ToList();

        product.ProductTags = product.ProductTags
                                     .Select(async x =>
                                     {
                                         try
                                         {
                                             return await _productTagRepository.GetProductTagByName(x.Name);
                                         }
                                         catch (EntityNotFoundException)
                                         {
                                             return x;
                                         }
                                     })
                                     .Select(t => t.Result)
                                     .ToList();

        await _productRepository.CreateProduct(product);
        var productDb = await _productRepository.GetProductById(product.Id);

        return productDb.Adapt<ProductReadDto>();
    }

    public async Task<ProductReadDto> GetProductById(string productId)
    {
        var product = await _productRepository.GetProductById(productId);
        return product.Adapt<ProductReadDto>();
    }

    public async Task<ICollection<ProductReadDto>> GetProducts(ProductFilter productFilter)
    {
        var products = await _productRepository.GetProducts(productFilter);
        return products.Adapt<ICollection<ProductReadDto>>();
    }
}
