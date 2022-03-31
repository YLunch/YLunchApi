using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProductRepository(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Create(Product product)
    {
        await _context.Products.AddAsync(product);
        var existingProduct = await _context.Products.FirstOrDefaultAsync(x => x.Name == product.Name);
        if (existingProduct != null)
        {
            throw new EntityAlreadyExistsException();
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Product> GetById(string productId)
    {
        var product = await ProductQueryBase.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null)
        {
            throw new EntityNotFoundException();
        }

        return FormatProduct(product);
    }

    public async Task<ICollection<Product>> GetProducts(ProductFilter productFilter)
    {
        var query = ProductQueryBase
                    .Skip((productFilter.Page - 1) * productFilter.Size)
                    .Take(productFilter.Size);
        query = FilterByRestaurantId(query, productFilter.RestaurantId);
        query = FilterByIsAvailable(query, productFilter.IsAvailable);

        var products = await query.ToListAsync();
        return products.Select(FormatProduct)
                       .OrderBy(x => x.Name)
                       .ToList();
    }

    private IQueryable<Product> FilterByIsAvailable(IQueryable<Product> query, bool? isAvailable) =>
        isAvailable switch
        {
            true => query.Where(x =>
                x.IsActive &&
                (x.Quantity > 1 || x.Quantity == null) &&
                (x.ExpirationDateTime == null || x.ExpirationDateTime >= _dateTimeProvider.UtcNow)),
            false => query.Where(x =>
                !x.IsActive ||
                x.Quantity == 0 ||
                (x.ExpirationDateTime != null && x.ExpirationDateTime < _dateTimeProvider.UtcNow)),
            null => query
        };

    private static IQueryable<Product> FilterByRestaurantId(IQueryable<Product> query, string? restaurantId) =>
        restaurantId switch
        {
            null => query,
            _ => query.Where(x => x.RestaurantId == restaurantId)
        };

    private IQueryable<Product> ProductQueryBase =>
        _context.Products
                .Include(x => x.Allergens)
                .Include(x => x.ProductTags);

    private static Product FormatProduct(Product product)
    {
        product.Allergens = product.Allergens.OrderBy(x => x.Name).ToList();
        product.ProductTags = product.ProductTags.OrderBy(x => x.Name).ToList();
        return product;
    }
}
