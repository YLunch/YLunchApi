using Microsoft.EntityFrameworkCore;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Infrastructure.Database;

namespace YLunchApi.AuthenticationShared.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Create(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByToken(string refreshToken)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
    }

    public async Task Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task Revoke(string accessTokenId)
    {
        var notRevokedRefreshTokens = await _context.RefreshTokens
                                                    .Where(x => x.JwtId == accessTokenId)
                                                    .Where(x => !x.IsRevoked)
                                                    .ToListAsync();

        foreach (var refreshToken in notRevokedRefreshTokens)
        {
            refreshToken.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }
}
