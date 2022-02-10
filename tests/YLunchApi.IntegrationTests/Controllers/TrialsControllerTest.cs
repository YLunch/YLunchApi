using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.UnitTests.Application.UserAggregate;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class TrialsControllerTest : ControllerTestBase
{
    [Fact]
    public async Task GetAnonymousTry_Should_Return_A_200Ok()
    {
        var response = await Client.GetAsync("trials/anonymous");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ResponseUtils.DeserializeContentAsync(response);

        content.Should().BeEquivalentTo("YLunchApi is running, you are anonymous");
    }

    [Fact]
    public async Task GetAuthenticatedTry_As_Customer_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUserInfo.AccessToken);

        // Act
        var authenticatedTrialResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialResponseContent = await ResponseUtils.DeserializeContentAsync(authenticatedTrialResponse);

        // Assert
        authenticatedTrialResponseContent.Should()
            .BeEquivalentTo(
                $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}");

        var refreshTokensBody = new
        {
            authenticatedUserInfo.AccessToken,
            authenticatedUserInfo.RefreshToken
        };
        var refreshTokensResponse =
            await Client.PostAsJsonAsync("authentication/refresh-tokens", refreshTokensBody);
        refreshTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshedTokens = await ResponseUtils.DeserializeContentAsync<TokenReadDto>(refreshTokensResponse);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshedTokens.AccessToken);
        var authenticatedTrialRefreshedTokensResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialRefreshedTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialWithExpiredTokensResponseContent =
            await ResponseUtils.DeserializeContentAsync(authenticatedTrialRefreshedTokensResponse);
        authenticatedTrialWithExpiredTokensResponseContent.Should()
            .BeEquivalentTo(
                $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}");
    }

    [Fact]
    public async Task GetAuthenticatedTry_As_RestaurantAdmin_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUserInfo.AccessToken);

        // Act
        var authenticatedTrialResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialResponseContent = await ResponseUtils.DeserializeContentAsync(authenticatedTrialResponse);

        // Assert
        authenticatedTrialResponseContent.Should()
            .BeEquivalentTo(
                $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}");

        var refreshTokensBody = new
        {
            authenticatedUserInfo.AccessToken,
            authenticatedUserInfo.RefreshToken
        };
        var refreshTokensResponse =
            await Client.PostAsJsonAsync("authentication/refresh-tokens", refreshTokensBody);
        refreshTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshedTokens = await ResponseUtils.DeserializeContentAsync<TokenReadDto>(refreshTokensResponse);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshedTokens.AccessToken);
        var authenticatedTrialRefreshedTokensResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialRefreshedTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialWithExpiredTokensResponseContent =
            await ResponseUtils.DeserializeContentAsync(authenticatedTrialRefreshedTokensResponse);
        authenticatedTrialWithExpiredTokensResponseContent.Should()
            .BeEquivalentTo(
                $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}");
    }

    [Fact]
    public async Task GetAuthenticatedTry_Should_Return_A_401Unauthorized_When_Missing_Authorization_Header()
    {
        // Act
        var response = await Client.GetAsync("trials/authenticated");

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task GetAuthenticatedTry_Should_Return_A_401Unauthorized_When_Invalid_Token()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "Invalid token");

        // Act
        var response = await Client.GetAsync("trials/authenticated");

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }


    [Fact]
    public async Task GetAuthenticatedRestaurantAdminTry_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-restaurant-admin");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ResponseUtils.DeserializeContentAsync(response);

        // Assert
        content.Should().BeEquivalentTo(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}");
    }

    [Fact]
    public async Task GetAuthenticatedRestaurantAdminTry_Should_Return_A_403Forbidden()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-restaurant-admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuthenticatedCustomerTry_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-customer");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ResponseUtils.DeserializeContentAsync(response);

        // Assert
        content.Should().BeEquivalentTo(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}");
    }

    [Fact]
    public async Task GetAuthenticatedCustomerTry_Should_Return_A_403Forbidden()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-customer");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
