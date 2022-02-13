using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.TestsShared.Models;
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class AuthenticationControllerTest
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly JwtService _jwtService;

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

        _jwtService = new JwtService(
            _refreshTokenRepository,
            optionsMonitorMock,
            tokenValidationParameter,
            _userRepository,
            new JwtSecurityTokenHandler()
        );
    }

    private AuthenticationController CreateAuthenticationController(
        IHttpContextAccessor httpContextAccessor)
    {
        return new AuthenticationController(_jwtService, _userService,
            httpContextAccessor);
    }

    private async Task<AuthenticatedUserInfo> Login(UserCreateDto user, string role)
    {
        // Arrange
        await _userService.Create(user, role);
        var userDb = await _userRepository.GetByEmail(user.Email);
        userDb = Assert.IsType<User>(userDb);

        var loginRequestDto = new LoginRequestDto
        {
            Email = user.Email,
            Password = user.Password
        };
        var controller = CreateAuthenticationController(HttpContextAccessorMocker.GetWithoutAuthorization());

        // Act
        var response = await controller.Login(loginRequestDto);
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<TokenReadDto>(responseResult.Value);
        var jwtSecurityToken = new ApplicationSecurityToken(responseBody.AccessToken);

        // Assert
        jwtSecurityToken.UserId.Should().BeEquivalentTo(userDb.Id);
        jwtSecurityToken.Subject.Should().BeEquivalentTo(userDb.Email);

        return new AuthenticatedUserInfo(responseBody.AccessToken, responseBody.RefreshToken);
    }

    [Fact]
    public async Task Login_Should_Return_A_200Ok_Containing_Tokens()
    {
        // Arrange, Act & Assert
        _ = await Login(UserMocks.RestaurantAdminCreateDto, Roles.RestaurantAdmin);
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
        var controller = CreateAuthenticationController(HttpContextAccessorMocker.GetWithoutAuthorization());

        // Act
        var response = await controller.Login(loginRequestDto);
        var responseResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);
        responseBody.Should().Be("Please login with valid credentials");
    }

    [Fact]
    public async Task RefreshTokens_Should_Return_A_200Ok_Containing_Tokens()
    {
        // Arrange
        var controller = CreateAuthenticationController(HttpContextAccessorMocker.GetWithoutAuthorization());

        var authenticatedUserInfo = await Login(UserMocks.RestaurantAdminCreateDto, Roles.RestaurantAdmin);

        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = authenticatedUserInfo.AccessToken,
            RefreshToken = authenticatedUserInfo.RefreshToken
        };

        // Act
        var response = await controller.RefreshTokens(refreshTokensRequest);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<TokenReadDto>(responseResult.Value);

        var newAuthenticatedUserInfo = new AuthenticatedUserInfo(responseBody.AccessToken, responseBody.RefreshToken);
        newAuthenticatedUserInfo.RefreshToken.Should().Be(responseBody.RefreshToken);
        newAuthenticatedUserInfo.UserId.Should().BeEquivalentTo(authenticatedUserInfo.UserId);
        newAuthenticatedUserInfo.Subject.Should().BeEquivalentTo(authenticatedUserInfo.UserEmail);

        var oldRefreshToken = await _refreshTokenRepository.GetByToken(refreshTokensRequest.RefreshToken);
        oldRefreshToken = Assert.IsType<RefreshToken>(oldRefreshToken);
        oldRefreshToken.IsUsed.Should().BeTrue();

        var newRefreshToken = await _refreshTokenRepository.GetByToken(responseBody.RefreshToken);
        newRefreshToken = Assert.IsType<RefreshToken>(newRefreshToken);
        newRefreshToken.UserId.Should().Be(newAuthenticatedUserInfo.UserId);
        newRefreshToken.JwtId.Should().Be(newAuthenticatedUserInfo.Id);
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
        var controller = CreateAuthenticationController(HttpContextAccessorMocker.GetWithoutAuthorization());

        // Act
        var response = await controller.RefreshTokens(refreshTokensRequest);

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
        var controller = CreateAuthenticationController(HttpContextAccessorMocker.GetWithoutAuthorization());

        // Act
        async Task Act() => await controller.RefreshTokens(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<RefreshTokenNotFoundException>(Act);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_200Ok_Containing_Current_User()
    {
        // Arrange
        var authenticatedUserInfo = await Login(UserMocks.RestaurantAdminCreateDto, Roles.RestaurantAdmin);

        var controller =
            CreateAuthenticationController(
                HttpContextAccessorMocker.GetWithAuthorization(authenticatedUserInfo.AccessToken));

        // Act
        var response = await controller.GetCurrentUser();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<UserReadDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(UserMocks.RestaurantAdminUserReadDto(authenticatedUserInfo.UserId));
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_401Unauthorized_When_User_Not_Found()
    {
        var controller =
            CreateAuthenticationController(
                HttpContextAccessorMocker.GetWithAuthorization(TokenMocks.ValidCustomerAccessToken));

        // Act
        var response = await controller.GetCurrentUser();

        // Assert
        var responseResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);
        responseBody.Should().Be("Invalid tokens, please login to generate new valid tokens");
    }
}
