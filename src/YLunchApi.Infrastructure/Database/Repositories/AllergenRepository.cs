using Microsoft.EntityFrameworkCore;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;

namespace YLunchApi.Infrastructure.Database.Repositories;

public class AllergenRepository : IAllergenRepository
{
    private readonly ApplicationDbContext _context;

    public AllergenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Allergen> GetByName(string name)
    {
        var allergen = await _context.Allergens.FirstOrDefaultAsync(x => x.Name == name);
        if (allergen == null)
        {
            throw new EntityNotFoundException();
        }

        return allergen;
    }
}
