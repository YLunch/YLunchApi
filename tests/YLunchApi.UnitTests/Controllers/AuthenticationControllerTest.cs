using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Authentication.Exceptions;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Authentication.Repositories;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.TestsShared.Models;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Controllers;

public class AuthenticationControllerTest : UnitTestFixture
{
    public AuthenticationControllerTest(UnitTestFixtureBase fixture) : base(fixture)
    {
    }

    private async Task<AuthenticatedUserInfo> Login(UserCreateDto user, string role)
    {
        // Arrange
        Fixture.InitFixture();
        var userService = Fixture.GetImplementationFromService<IUserService>();
        var userRepository = Fixture.GetImplementationFromService<IUserRepository>();

        await userService.Create(user, role);
        var userDb = await userRepository.GetByEmail(user.Email);
        userDb = Assert.IsType<User>(userDb);

        var loginRequestDto = new LoginRequestDto
        {
            Email = user.Email,
            Password = user.Password
        };
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

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
        // Arrange
        Fixture.InitFixture();

        // Act & Assert
        _ = await Login(UserMocks.RestaurantAdminCreateDto, Roles.RestaurantAdmin);
    }

    [Fact]
    public async Task Login_Should_Return_A_401Unauthorized()
    {
        // Arrange
        Fixture.InitFixture();
        var user = UserMocks.RestaurantAdminCreateDto;

        var loginRequestDto = new LoginRequestDto
        {
            Email = user.Email,
            Password = user.Password
        };
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

        // Act
        var response = await controller.Login(loginRequestDto);
        var responseResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new MessageDto("Please login with valid credentials."));
    }

    [Fact]
    public async Task RefreshTokens_Should_Return_A_200Ok_Containing_Tokens()
    {
        // Arrange
        Fixture.InitFixture();
        var refreshTokenRepository = Fixture.GetImplementationFromService<IRefreshTokenRepository>();
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

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

        var oldRefreshToken = await refreshTokenRepository.GetByToken(refreshTokensRequest.RefreshToken);
        oldRefreshToken = Assert.IsType<RefreshToken>(oldRefreshToken);
        oldRefreshToken.IsUsed.Should().BeTrue();

        var newRefreshToken = await refreshTokenRepository.GetByToken(responseBody.RefreshToken);
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
        Fixture.InitFixture();
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = TokenMocks.RefreshToken.Token
        };
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

        // Act
        var response = await controller.RefreshTokens(refreshTokensRequest);

        // Assert
        var responseResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new MessageDto("Invalid tokens, please login to generate new valid tokens."));
    }

    [Fact]
    public async Task RefreshTokens_Should_Throw_RefreshTokenNotFoundException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Fixture.InitFixture();
        var refreshTokensRequest = new TokenUpdateDto
        {
            AccessToken = TokenMocks.ExpiredAccessToken,
            RefreshToken = TokenMocks.RefreshToken.Token
        };
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

        // Act
        async Task Act() => await controller.RefreshTokens(refreshTokensRequest);

        // Assert
        await Assert.ThrowsAsync<RefreshTokenNotFoundException>(Act);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_200Ok_Containing_Current_User_RestaurantAdmin()
    {
        // Arrange
        var authenticatedUserInfo = await Login(UserMocks.RestaurantAdminCreateDto, Roles.RestaurantAdmin);
        Fixture.InitFixture(configuration => configuration.AccessToken = authenticatedUserInfo.AccessToken);
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

        // Act
        var response = await controller.GetCurrentUser();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<UserReadDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(UserMocks.RestaurantAdminUserReadDto(authenticatedUserInfo.UserId));
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_200Ok_Containing_Current_User_Customer()
    {
        // Arrange
        var authenticatedUserInfo = await Login(UserMocks.CustomerCreateDto, Roles.Customer);
        Fixture.InitFixture(configuration => configuration.AccessToken = authenticatedUserInfo.AccessToken);
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

        // Act
        var response = await controller.GetCurrentUser();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<UserReadDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(UserMocks.CustomerUserReadDto(authenticatedUserInfo.UserId));
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_401Unauthorized_When_User_Not_Found()
    {
        Fixture.InitFixture(configuration => configuration.AccessToken = TokenMocks.ValidCustomerAccessToken);
        var controller = Fixture.GetImplementationFromService<AuthenticationController>();

        // Act
        var response = await controller.GetCurrentUser();

        // Assert
        var responseResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new MessageDto("Invalid tokens, please login to generate new valid tokens."));
    }
}
