using YLunchApi.Authentication.Models;

namespace YLunchApi.Authentication.Repositories;

public interface IRefreshTokenRepository
{
    Task Create(RefreshToken refreshToken);
    Task<RefreshToken?> GetByToken(string refreshToken);
    Task Update(RefreshToken refreshToken);
    Task Revoke(string accessTokenId);
}
