using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
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
        var product = await _context.Products
                                    .Include(x => x.Allergens)
                                    .Include(x => x.ProductTags)
                                    .FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null)
        {
            throw new EntityNotFoundException();
        }

        return FormatProduct(product);
    }

    private static Product FormatProduct(Product product)
    {
        product.Allergens = product.Allergens.OrderBy(x => x.Name).ToList();
        product.ProductTags = product.ProductTags.OrderBy(x => x.Name).ToList();
        return product;
    }
}
