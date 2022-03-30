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

    public async Task<ProductReadDto> Create(ProductCreateDto productCreateDto, string restaurantId)
    {
        var product = productCreateDto.Adapt<Product>();
        product.RestaurantId = restaurantId;
        product.CreationDateTime = _dateTimeProvider.UtcNow;

        product.Allergens = product.Allergens
                                   .Select(async x =>
                                   {
                                       try
                                       {
                                           return await _allergenRepository.GetByName(x.Name);
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
                                             return await _productTagRepository.GetByName(x.Name);
                                         }
                                         catch (EntityNotFoundException)
                                         {
                                             return x;
                                         }
                                     })
                                     .Select(t => t.Result)
                                     .ToList();

        await _productRepository.Create(product);
        var productDb = await _productRepository.GetById(product.Id);

        return productDb.Adapt<ProductReadDto>();
    }

    public async Task<ProductReadDto> GetById(string productId)
    {
        var product = await _productRepository.GetById(productId);
        return product.Adapt<ProductReadDto>();
    }

    public async Task<ICollection<ProductReadDto>> GetProducts(ProductFilter productFilter)
    {
        var products = await _productRepository.GetProducts(productFilter);
        return products.Adapt<ICollection<ProductReadDto>>();
    }
}
