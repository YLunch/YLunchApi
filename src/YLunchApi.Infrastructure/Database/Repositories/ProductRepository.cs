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

    public async Task CreateProduct(Product product)
    {
        await _context.Products.AddAsync(product);
        var existingProduct = await _context.Products
                                            .Where(x => x.RestaurantId == product.RestaurantId)
                                            .FirstOrDefaultAsync(x => x.Name == product.Name);
        if (existingProduct != null)
        {
            throw new EntityAlreadyExistsException();
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Product> GetProductById(string productId)
    {
        var product = await ProductsQueryBase.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null)
        {
            throw new EntityNotFoundException();
        }

        return FormatProduct(product);
    }

    public async Task<ICollection<Product>> GetProducts(ProductFilter productFilter)
    {
        var query = FilterByRestaurantId(ProductsQueryBase, productFilter.RestaurantId);
        query = FilterByIsAvailable(query, productFilter.IsAvailable);
        query = FilterByProductIds(query, productFilter.ProductIds);

        var products = await query
                             .Skip((productFilter.Page - 1) * productFilter.Size)
                             .Take(productFilter.Size)
                             .ToListAsync();
        return products.Select(FormatProduct)
                       .OrderBy(x => x.Name)
                       .ToList();
    }

    public IQueryable<Product> ProductsQueryBase =>
        _context.Products
                .Include(x => x.Allergens)
                .Include(x => x.ProductTags);

    private static IQueryable<Product> FilterByRestaurantId(IQueryable<Product> query, string? restaurantId) =>
        restaurantId switch
        {
            null => query,
            _ => query.Where(x => x.RestaurantId == restaurantId)
        };

    private IQueryable<Product> FilterByIsAvailable(IQueryable<Product> query, bool? isAvailable) =>
        isAvailable switch
        {
            null => query,
            true => query.Where(x =>
                x.IsActive &&
                (x.Quantity > 1 || x.Quantity == null) &&
                (x.ExpirationDateTime == null || x.ExpirationDateTime >= _dateTimeProvider.UtcNow)),
            false => query.Where(x =>
                !x.IsActive ||
                x.Quantity == 0 ||
                x.ExpirationDateTime != null && x.ExpirationDateTime < _dateTimeProvider.UtcNow)
        };

    private static IQueryable<Product> FilterByProductIds(IQueryable<Product> query, SortedSet<string>? productIds) =>
        productIds switch
        {
            null => query,
            _ => query.Where(x => productIds.Contains(x.Id))
        };

    private static Product FormatProduct(Product product)
    {
        product.Allergens = product.Allergens.OrderBy(x => x.Name).ToList();
        product.ProductTags = product.ProductTags.OrderBy(x => x.Name).ToList();
        return product;
    }
}
