using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.IntegrationTests.Core.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class AuthenticationControllerITest : ControllerITestBase
{
    #region LoginTests

    [Fact]
    public async Task Login_Should_Return_A_200Ok()
    {
        // Arrange
        var user = await CreateUser(UserMocks.CustomerCreateDto);

        // Act and Assert
        _ = await LoginUser(user.Email, UserMocks.CustomerCreateDto.Password);
    }

    [Fact]
    public async Task Login_Should_Return_A_400BadRequest_When_Missing_Fields()
    {
        // Arrange, Act and Assert
        var body = new
        {
            Email = "",
            Password = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("authentication/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should()
               .Contain("The Email field is required.")
               .And
               .Contain("The Password field is required.");
    }

    [Fact]
    public async Task Login_Should_Return_A_400BadRequest_When_Email_Is_Invalid()
    {
        // Arrange, Act and Assert
        var body = new
        {
            Email = "Invalid Email",
            UserMocks.CustomerCreateDto.Password
        };

        // Act
        var response = await Client.PostAsJsonAsync("authentication/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should()
               .Contain("Email is invalid.");
    }

    #endregion

    #region LogoutTests

    [Fact]
    public async Task Logout_Should_Return_A_204NoContent_And_Logout_Current_User_RestaurantAdmin()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);

        Client.SetAuthorizationHeader(decodedTokens.AccessToken);

        // Act
        var response = await Client.GetAsync("authentication/logout");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var body = new
        {
            decodedTokens.AccessToken,
            decodedTokens.RefreshToken
        };
        var refreshTokensResponse = await Client.PostAsJsonAsync("authentication/refresh-tokens", body);

        refreshTokensResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await ResponseUtils.DeserializeContentAsync<ErrorDto>(refreshTokensResponse);
        content.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.Unauthorized,
            "Invalid tokens, please login to generate new valid tokens."));
    }

    [Fact]
    public async Task Logout_Should_Return_A_204NoContent_And_Logout_Current_User_Customer()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        Client.SetAuthorizationHeader(decodedTokens.AccessToken);

        // Act
        var response = await Client.GetAsync("authentication/logout");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var body = new
        {
            decodedTokens.AccessToken,
            decodedTokens.RefreshToken
        };
        var refreshTokensResponse = await Client.PostAsJsonAsync("authentication/refresh-tokens", body);

        refreshTokensResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await ResponseUtils.DeserializeContentAsync<ErrorDto>(refreshTokensResponse);
        content.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.Unauthorized,
            "Invalid tokens, please login to generate new valid tokens."));
    }

    #endregion

    #region RefreshTokensTests

    [Fact]
    public async Task RefreshTokens_Should_Return_A_200Ok()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var refreshTokensBody = new
        {
            decodedTokens.AccessToken,
            decodedTokens.RefreshToken
        };

        // Act
        var refreshTokensResponse = await Client.PostAsJsonAsync("authentication/refresh-tokens", refreshTokensBody);
        refreshTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshedTokens = await ResponseUtils.DeserializeContentAsync<TokenReadDto>(refreshTokensResponse);

        // Assert
        Assert.IsType<string>(refreshedTokens.AccessToken);
        Assert.IsType<string>(refreshedTokens.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokens_Should_Return_A_400BadRequest_When_Missing_Fields()
    {
        // Arrange, Act and Assert
        var body = new
        {
            AccessToken = "",
            RefreshToken = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("authentication/refresh-tokens", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should()
               .Contain("The AccessToken field is required.")
               .And
               .Contain("The RefreshToken field is required.");
    }

    [Fact]
    public async Task RefreshTokens_Should_Return_A_401Unauthorized()
    {
        // Arrange, Act and Assert
        var body = new
        {
            AccessToken = "Invalid Token",
            RefreshToken = "Invalid Token"
        };

        // Act
        var response = await Client.PostAsJsonAsync("authentication/refresh-tokens", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await ResponseUtils.DeserializeContentAsync<ErrorDto>(response);
        content.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.Unauthorized,
            "Invalid tokens, please login to generate new valid tokens."));
    }

    #endregion

    #region GetCurrentUserTests

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_200Ok()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(decodedTokens.AccessToken);

        // Act
        var response = await Client.GetAsync("authentication/current-user");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<UserReadDto>(response);

        // Assert
        responseBody.Should().BeEquivalentTo(UserMocks.CustomerUserReadDto(decodedTokens.UserId));
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_401Unauthorized_When_No_Header()
    {
        // Act
        var response = await Client.GetAsync("authentication/current-user");

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_A_401Unauthorized_When_User_Not_Found()
    {
        // Arrange
        Client.SetAuthorizationHeader(TokenMocks.ValidCustomerAccessToken);

        // Act
        var response = await Client.GetAsync("authentication/current-user");

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    #endregion
}
