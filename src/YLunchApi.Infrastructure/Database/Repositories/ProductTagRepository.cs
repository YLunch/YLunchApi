using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class ProductTagRepository : IProductTagRepository
{
    private readonly ApplicationDbContext _context;

    public ProductTagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductTag> GetProductTagByName(string name)
    {
        var productTag = await _context.ProductTags.FirstOrDefaultAsync(x => x.Name == name);
        if (productTag == null)
        {
            throw new EntityNotFoundException();
        }

        return productTag;
    }
}
