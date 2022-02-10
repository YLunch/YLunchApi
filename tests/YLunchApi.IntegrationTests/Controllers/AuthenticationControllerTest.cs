using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.UnitTests.Application.UserAggregate;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class AuthenticationControllerTest : ControllerTestBase
{
    [Fact]
    public async Task Login_Should_Return_A_200Ok()
    {
        // Arrange, Act and Assert
        _ = await Authenticate(UserMocks.CustomerCreateDto);
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

    [Fact]
    public async Task Refresh_Tokens_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);

        var refreshTokensBody = new
        {
            authenticatedUserInfo.AccessToken,
            authenticatedUserInfo.RefreshToken
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
    public async Task Refresh_Tokens_Should_Return_A_400BadRequest_When_Missing_Fields()
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
    public async Task Refresh_Tokens_Should_Return_A_401Unauthorized()
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
        var content = await ResponseUtils.DeserializeContentAsync(response);
        content.Should().Be("Invalid tokens, please login to generate new valid tokens");
    }
}
