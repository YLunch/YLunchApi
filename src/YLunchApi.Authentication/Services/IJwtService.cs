using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Domain.UserAggregate;

namespace YLunchApi.Authentication.Services;

public interface IJwtService
{
    Task<TokenReadDto> GenerateJwtToken(AuthenticatedUser authenticatedUser);
    Task<TokenReadDto> RefreshJwtToken(TokenUpdateDto tokenUpdateDto);
}