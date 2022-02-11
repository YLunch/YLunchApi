using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NSubstitute;
using Xunit;
using YLunchApi.Authentication.Exceptions;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Authentication.Services;
using YLunchApi.AuthenticationShared.Repositories;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.Infrastructure.Database;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Application.Authentication;

public class JwtServiceTest
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

    public JwtServiceTest()
    {
        _context = ContextBuilder.BuildContext();

        var roleManagerMock = ManagerMocker.GetRoleManagerMock(_context);
        var userManagerMock = ManagerMocker.GetUserManagerMock(_context);

        IUserRepository userRepository = new UserRepository(_context, userManagerMock.Object, roleManagerMock.Object);

        const string jwtSecret = "JsonWebTokenSecretForTests";
        var optionsMonitorMock = Substitute.For<IOptionsMonitor<JwtConfig>>();
        optionsMonitorMock.CurrentValue.Returns(new JwtConfig
        {
            Secret = jwtSecret
        });

        var key = Encoding.ASCII.GetBytes(jwtSecret);

        var tokenValidationParameter = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            RequireExpirationTime = false,

            // Allow to use seconds for expiration of token
            // Required only when token lifetime less than 5 minutes
            ClockSkew = TimeSpan.Zero
        };

        _refreshTokenRepositoryMock = new Mock<RefreshTokenRepository>(_context).As<IRefreshTokenRepository>();

        _refreshTokenRepositoryMock.Setup(x => x.GetByToken(It.IsAny<string>()))
            .Returns<string>(async refreshTokenString =>
                {
                    return await _context.RefreshTokens
                        .FirstOrDefaultAsync(x => x.Token.Equals(refreshTokenString));
                }
            );

        _jwtService = new JwtService(
            _refreshTokenRepositoryMock.Object,
            optionsMonitorMock,
            tokenValidationParameter,
            userRepository
        );
    }

    [Fact]
    public async Task RefreshJwtToken_Should_Throw_InvalidTokenException_When_AccessToken_Has_Bad_Algorithm_Encryption()
    {
        // Arrange
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.BadAlgorithmAccessToken,
            RefreshToken = TokenMocks.RefreshToken.Token
        };

        // Act
        async Task Act() => await _jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<InvalidTokenException>(Act);
    }

    [Fact]
    public async Task RefreshJwtToken_Should_Throw_RefreshTokenNotFoundException()
    {
        // Arrange
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = "unknownToken"
        };

        // Act
        async Task Act() => await _jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<RefreshTokenNotFoundException>(Act);
    }

    [Fact]
    public async Task RefreshJwtToken_Should_Throw_RefreshTokenExpiredException()
    {
        // Arrange
        var refreshToken = TokenMocks.RefreshToken;
        refreshToken.ExpirationDateTime = refreshToken.ExpirationDateTime.AddMonths(-1);
        await _context.RefreshTokens.AddAsync(refreshToken);
        _refreshTokenRepositoryMock.Setup(x => x.GetByToken(It.IsAny<string>()))
            .ReturnsAsync(() => refreshToken);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };

        // Act
        async Task Act() => await _jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<RefreshTokenExpiredException>(Act);
    }


    [Fact]
    public async Task RefreshJwtToken_Should_Throw_RefreshTokenAlreadyUsedException()
    {
        // Arrange
        var refreshToken = TokenMocks.RefreshToken;
        refreshToken.IsUsed = true;
        await _context.AddAsync(refreshToken);
        _refreshTokenRepositoryMock.Setup(x => x.GetByToken(It.IsAny<string>()))
            .ReturnsAsync(() => refreshToken);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };

        // Act
        async Task Act() => await _jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<RefreshTokenAlreadyUsedException>(Act);
    }

    [Fact]
    public async Task RefreshJwtToken_Should_Throw_RefreshTokenRevokedException()
    {
        // Arrange
        var refreshToken = TokenMocks.RefreshToken;
        refreshToken.IsRevoked = true;
        await _context.AddAsync(refreshToken);
        _refreshTokenRepositoryMock.Setup(x => x.GetByToken(It.IsAny<string>()))
            .ReturnsAsync(() => refreshToken);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };

        // Act
        async Task Act() => await _jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<RefreshTokenRevokedException>(Act);
    }

    [Fact]
    public async Task RefreshJwtToken_Should_Throw_AccessAndRefreshTokensNotMatchException()
    {
        // Arrange
        var refreshToken = TokenMocks.RefreshToken;
        refreshToken.JwtId = "badId";
        await _context.AddAsync(refreshToken);
        _refreshTokenRepositoryMock.Setup(x => x.GetByToken(It.IsAny<string>()))
            .ReturnsAsync(() => refreshToken);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };

        // Act
        async Task Act() => await _jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<AccessAndRefreshTokensNotMatchException>(Act);
    }
}
