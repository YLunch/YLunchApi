using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YLunchApi.Authentication.Exceptions;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Authentication.Utils;
using YLunchApi.Domain.UserAggregate;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace YLunchApi.Authentication.Services;

public class JwtService : IJwtService
{
    private readonly JwtConfig _jwtConfig;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IUserRepository _userRepository;

    public JwtService(IRefreshTokenRepository refreshTokenRepository,
        IOptionsMonitor<JwtConfig> jwtConfig, TokenValidationParameters tokenValidationParameters,
        IUserRepository userRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenValidationParameters = tokenValidationParameters;
        _userRepository = userRepository;
        _jwtConfig = jwtConfig.CurrentValue;
    }

    public async Task<TokenReadDto> GenerateJwtToken(AuthenticatedUser authenticatedUser)
    {
        var accessToken = CreateToken(authenticatedUser);

        var refreshToken = await CreateRefreshToken(authenticatedUser.Id, accessToken.SecurityToken.Id);

        return new TokenReadDto(accessToken.StringToken, refreshToken.Token);
    }

    public async Task<TokenReadDto> RefreshJwtToken(TokenUpdateDto tokenUpdateDto)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var tokenValidationParameters = _tokenValidationParameters.Clone();
        tokenValidationParameters.ValidateLifetime = false;


        // Validation 1 - jwt format
        var tokenInValidation = jwtTokenHandler.ValidateToken(
            tokenUpdateDto.AccessToken,
            tokenValidationParameters,
            out var validatedToken
        );

        if (tokenInValidation == null || validatedToken == null) throw new InvalidTokenException();

        // Validation 2 - encryption algorithm
        if (validatedToken is JwtSecurityToken jwtSecurityToken)
        {
            var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase);
            if (!result) throw new InvalidTokenException();
        }

        // Validation 3 - refresh token exists
        var storedToken = await _refreshTokenRepository.GetByToken(tokenUpdateDto.RefreshToken);
        if (storedToken == null) throw new RefreshTokenNotFoundException();

        // Validation 4 - refresh token is not expired
        if (storedToken.ExpirationDateTime < DateTime.UtcNow) throw new RefreshTokenExpiredException();

        // Validation 5 - refresh token not used
        if (storedToken.IsUsed) throw new RefreshTokenAlreadyUsedException();

        // Validation 6 - refresh token not revoked
        if (storedToken.IsRevoked) throw new RefreshTokenRevokedException();

        // Validation 7 - refresh token has a valid Id
        var jti = tokenInValidation.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)!.Value;
        if (storedToken.JwtId != jti) throw new AccessAndRefreshTokensNotMatchException();

        // Update current refresh token
        storedToken.IsUsed = true;
        await _refreshTokenRepository.Update(storedToken);

        // Create new access and refresh tokens
        var userDb = await _userRepository.GetById(storedToken.UserId);
        var userRoles = await _userRepository.GetUserRoles(userDb!);
        var authenticatedUser = new AuthenticatedUser(userDb!, userRoles);
        return await GenerateJwtToken(authenticatedUser);
    }

    private Token CreateToken(AuthenticatedUser authenticatedUser)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", authenticatedUser.Id),
                new Claim(JwtRegisteredClaimNames.Sub, authenticatedUser.UserName),
                new Claim("Roles", string.Join(";", authenticatedUser.Roles)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = jwtTokenHandler.CreateToken(tokenDescriptor);
        var stringToken = jwtTokenHandler.WriteToken(securityToken);

        var accessToken = new Token(securityToken, stringToken);

        return accessToken;
    }

    private async Task<RefreshToken> CreateRefreshToken(string userId, string tokenId)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            JwtId = tokenId,
            IsUsed = false,
            IsRevoked = false,
            UserId = userId,
            CreationDateTime = DateTime.UtcNow,
            ExpirationDateTime = DateTime.UtcNow.AddMonths(1),
            Token = RandomUtils.GetRandomKey() + Guid.NewGuid()
        };

        await _refreshTokenRepository.Create(refreshToken);
        return refreshToken;
    }
}
