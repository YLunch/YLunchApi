using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;
using YLunchApi.Application.UserAggregate;
using YLunchApi.Authentication.Exceptions;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Authentication.Services;
using YLunchApi.AuthenticationShared.Repositories;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.Main.Controllers;
using YLunchApi.UnitTests.Application.Authentication;
using YLunchApi.UnitTests.Application.UserAggregate;
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class AuthenticationControllerTest
{
    private readonly AuthenticationController _authenticationController;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;

    public AuthenticationControllerTest()
    {
        var context = ContextBuilder.BuildContext();

        var roleManagerMock = ManagerMocker.GetRoleManagerMock(context);
        var userManagerMock = ManagerMocker.GetUserManagerMock(context);

        _userRepository = new UserRepository(context, userManagerMock.Object, roleManagerMock.Object);
        _userService = new UserService(_userRepository);

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

        _refreshTokenRepository = new RefreshTokenRepository(context);

        IJwtService jwtService = new JwtService(
            _refreshTokenRepository,
            optionsMonitorMock,
            tokenValidationParameter,
            _userRepository
        );
        _authenticationController =
            new AuthenticationController(jwtService, _userService, HttpContextAccessorMocker.GetWithoutAuthorization());
    }

    [Fact]
    public async Task Login_Should_Return_A_200Ok_Containing_Tokens()
    {
        // Arrange
        var user = UserMocks.RestaurantAdminCreateDto;

        await _userService.Create(user, Roles.RestaurantAdmin);
        var userDb = await _userRepository.GetByEmail(user.Email);
        userDb = Assert.IsType<User>(userDb);

        var loginRequestDto = new LoginRequestDto
        {
            Email = user.Email,
            Password = user.Password
        };

        // Act
        var response = await _authenticationController.Login(loginRequestDto);
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<TokenReadDto>(responseResult.Value);
        var jwtSecurityToken = new ApplicationSecurityToken(responseBody.AccessToken);

        jwtSecurityToken.UserId.Should().BeEquivalentTo(userDb.Id);
        jwtSecurityToken.Subject.Should().BeEquivalentTo(userDb.Email);
    }

    [Fact]
    public async Task Login_Should_Return_A_401Unauthorized()
    {
        // Arrange
        var user = UserMocks.RestaurantAdminCreateDto;

        var loginRequestDto = new LoginRequestDto
        {
            Email = user.Email,
            Password = user.Password
        };

        // Act
        var response = await _authenticationController.Login(loginRequestDto);
        var responseResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);
        responseBody.Should().Be("Please login with valid credentials");
    }


    [Fact]
    public async Task RefreshTokens_Should_Return_A_200Ok_Containing_Tokens()
    {
        // Arrange
        var user = UserMocks.RestaurantAdminCreateDto;

        await _userService.Create(user, Roles.RestaurantAdmin);
        var userDb = await _userRepository.GetByEmail(user.Email);
        userDb = Assert.IsType<User>(userDb);

        var loginRequestDto = new LoginRequestDto
        {
            Email = user.Email,
            Password = user.Password
        };

        var loginResponse = await _authenticationController.Login(loginRequestDto);

        var loginResponseResult = Assert.IsType<OkObjectResult>(loginResponse.Result);
        var loginTokens = Assert.IsType<TokenReadDto>(loginResponseResult.Value);

        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = loginTokens.AccessToken,
            RefreshToken = loginTokens.RefreshToken
        };

        // Act
        var response = await _authenticationController.RefreshTokens(refreshTokensRequest);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<TokenReadDto>(responseResult.Value);

        var jwtSecurityToken = new ApplicationSecurityToken(responseBody.AccessToken);
        jwtSecurityToken.UserId.Should().BeEquivalentTo(userDb.Id);
        jwtSecurityToken.Subject.Should().BeEquivalentTo(userDb.Email);

        var oldRefreshToken = await _refreshTokenRepository.GetByToken(refreshTokensRequest.RefreshToken);
        oldRefreshToken = Assert.IsType<RefreshToken>(oldRefreshToken);
        oldRefreshToken.IsUsed.Should().BeTrue();

        var newRefreshToken = await _refreshTokenRepository.GetByToken(responseBody.RefreshToken);
        newRefreshToken = Assert.IsType<RefreshToken>(newRefreshToken);
        newRefreshToken.UserId.Should().Be(jwtSecurityToken.UserId);
        newRefreshToken.JwtId.Should().Be(jwtSecurityToken.Id);
        newRefreshToken.IsRevoked.Should().BeFalse();
        newRefreshToken.IsUsed.Should().BeFalse();
        newRefreshToken.CreationDateTime.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1));
        newRefreshToken.CreationDateTime.Should().BeBefore(DateTime.UtcNow);
        newRefreshToken.ExpirationDateTime.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1).AddMonths(1));
        newRefreshToken.ExpirationDateTime.Should().BeBefore(DateTime.UtcNow.AddMonths(1));
        Assert.IsType<string>(newRefreshToken.Id);
        Assert.IsType<string>(newRefreshToken.Token);
        newRefreshToken.Id.Should().NotBe(oldRefreshToken.Id);
        newRefreshToken.Token.Should().NotBe(oldRefreshToken.Token);
    }

    [Fact]
    public async Task RefreshTokens_Should_Return_A_401Unauthorized_When_AccessToken_Is_Bad_Signed()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = TokenMocks.RefreshToken.Token
        };

        // Act
        var response = await _authenticationController.RefreshTokens(refreshTokensRequest);

        // Assert
        var responseResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);
        responseBody.Should().Be("Invalid tokens, please login to generate new valid tokens");
    }

    [Fact]
    public async Task RefreshTokens_Should_Throw_RefreshTokenNotFoundException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = TokenMocks.RefreshToken.Token
        };

        // Act
        async Task Act() => await _authenticationController.RefreshTokens(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<RefreshTokenNotFoundException>(Act);
    }
}
