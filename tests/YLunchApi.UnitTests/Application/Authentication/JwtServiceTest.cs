using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using YLunchApi.Authentication.Exceptions;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Authentication.Services;
using YLunchApi.AuthenticationShared.Repositories;
using YLunchApi.Infrastructure.Database;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Application.Authentication;

public class JwtServiceTest : UnitTestFixture
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

    public JwtServiceTest(UnitTestFixtureBase fixture) : base(fixture)
    {
        Fixture.InitFixture();
        _context = Fixture.GetImplementationFromService<ApplicationDbContext>();
        _refreshTokenRepositoryMock = new Mock<RefreshTokenRepository>(_context).As<IRefreshTokenRepository>();
        _refreshTokenRepositoryMock.Setup(x => x.GetByToken(It.IsAny<string>()))
                                   .Returns<string>(async refreshTokenString =>
                                       await _context.RefreshTokens
                                                     .FirstOrDefaultAsync(x =>
                                                         x.Token == refreshTokenString)
                                   );

        Fixture.InitFixture(configuration => configuration.RefreshTokenRepository = _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task RefreshJwtToken_Should_Throw_InvalidTokenException_When_ValidatedToken_Is_Null()
    {
        // Arrange
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.BadAlgorithmAccessToken,
            RefreshToken = TokenMocks.RefreshToken.Token
        };
        SecurityToken validatedToken;
        var jwtSecurityTokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
        jwtSecurityTokenHandlerMock
            .Setup(x =>
                x.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out validatedToken))
            .Returns(new ClaimsPrincipal());

        Fixture.InitFixture(configuration =>
            configuration.JwtSecurityTokenHandler = jwtSecurityTokenHandlerMock.Object);
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<InvalidTokenException>(Act);
    }

    [Fact]
    public async Task RefreshJwtToken_Should_Throw_InvalidTokenException_When_TokenInValidation_Is_Null()
    {
        // Arrange
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.BadAlgorithmAccessToken,
            RefreshToken = TokenMocks.RefreshToken.Token
        };
        SecurityToken validatedToken = new JwtSecurityToken();
        var jwtSecurityTokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
        jwtSecurityTokenHandlerMock
            .Setup(x =>
                x.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out validatedToken))
            .Returns<ClaimsPrincipal>(null);

        Fixture.InitFixture(configuration =>
            configuration.JwtSecurityTokenHandler = jwtSecurityTokenHandlerMock.Object);
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<InvalidTokenException>(Act);
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
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

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
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

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
        Fixture.InitFixture(configuration => configuration.RefreshTokenRepository = _refreshTokenRepositoryMock.Object);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

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
        Fixture.InitFixture(configuration => configuration.RefreshTokenRepository = _refreshTokenRepositoryMock.Object);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

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
        Fixture.InitFixture(configuration => configuration.RefreshTokenRepository = _refreshTokenRepositoryMock.Object);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

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
        Fixture.InitFixture(configuration => configuration.RefreshTokenRepository = _refreshTokenRepositoryMock.Object);
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = refreshToken.Token
        };
        var jwtService = Fixture.GetImplementationFromService<IJwtService>();

        // Act
        async Task Act() => await jwtService.RefreshJwtToken(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<AccessAndRefreshTokensNotMatchException>(Act);
    }
}
