using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.IntegrationTests.Core.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class TrialsControllerITest : ControllerITestBase
{
    #region GetAnonymousTry_Tests

    [Fact]
    public async Task GetAnonymousTry_Should_Return_A_200Ok()
    {
        var response = await Client.GetAsync("trials/anonymous");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ResponseUtils.DeserializeContentAsync<MessageDto>(response);

        content.Should().BeEquivalentTo(new MessageDto("YLunchApi is running, you are anonymous."));
    }

    #endregion

    #region GetAuthenticatedTry_Tests

    [Fact]
    public async Task GetAuthenticatedTry_As_Customer_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);

        // Act
        var authenticatedTrialResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialResponseContent =
            await ResponseUtils.DeserializeContentAsync<MessageDto>(authenticatedTrialResponse);

        // Assert
        authenticatedTrialResponseContent.Should()
                                         .BeEquivalentTo(new MessageDto(
                                             $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}."));

        var refreshTokensBody = new
        {
            authenticatedUserInfo.AccessToken,
            authenticatedUserInfo.RefreshToken
        };
        var refreshTokensResponse =
            await Client.PostAsJsonAsync("authentication/refresh-tokens", refreshTokensBody);
        refreshTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshedTokens = await ResponseUtils.DeserializeContentAsync<TokenReadDto>(refreshTokensResponse);

        Client.SetAuthorizationHeader(refreshedTokens.AccessToken);
        var authenticatedTrialRefreshedTokensResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialRefreshedTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialWithExpiredTokensResponseContent =
            await ResponseUtils.DeserializeContentAsync<MessageDto>(authenticatedTrialRefreshedTokensResponse);
        authenticatedTrialWithExpiredTokensResponseContent.Should()
                                                          .BeEquivalentTo(new MessageDto(
                                                              $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}."));
    }

    [Fact]
    public async Task GetAuthenticatedTry_As_RestaurantAdmin_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);

        // Act
        var authenticatedTrialResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialResponseContent =
            await ResponseUtils.DeserializeContentAsync<MessageDto>(authenticatedTrialResponse);

        // Assert
        authenticatedTrialResponseContent.Should()
                                         .BeEquivalentTo(new MessageDto(
                                             $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}."));

        var refreshTokensBody = new
        {
            authenticatedUserInfo.AccessToken,
            authenticatedUserInfo.RefreshToken
        };
        var refreshTokensResponse =
            await Client.PostAsJsonAsync("authentication/refresh-tokens", refreshTokensBody);
        refreshTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshedTokens = await ResponseUtils.DeserializeContentAsync<TokenReadDto>(refreshTokensResponse);

        Client.SetAuthorizationHeader(refreshedTokens.AccessToken);
        var authenticatedTrialRefreshedTokensResponse = await Client.GetAsync("trials/authenticated");
        authenticatedTrialRefreshedTokensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authenticatedTrialWithExpiredTokensResponseContent =
            await ResponseUtils.DeserializeContentAsync<MessageDto>(authenticatedTrialRefreshedTokensResponse);
        authenticatedTrialWithExpiredTokensResponseContent.Should()
                                                          .BeEquivalentTo(new MessageDto(
                                                              $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}."));
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
        Client.SetAuthorizationHeader("Invalid token");

        // Act
        var response = await Client.GetAsync("trials/authenticated");

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    #endregion

    #region GetAuthenticatedRestaurantAdminTry_Tests

    [Fact]
    public async Task GetAuthenticatedRestaurantAdminTry_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-restaurant-admin");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ResponseUtils.DeserializeContentAsync<MessageDto>(response);

        // Assert
        content.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}."));
    }

    [Fact]
    public async Task GetAuthenticatedRestaurantAdminTry_Should_Return_A_403Forbidden()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-restaurant-admin");

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    #endregion

    #region GetAuthenticatedCustomerTry_Tests

    [Fact]
    public async Task GetAuthenticatedCustomerTry_Should_Return_A_200Ok()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-customer");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ResponseUtils.DeserializeContentAsync<MessageDto>(response);

        // Assert
        content.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}."));
    }

    [Fact]
    public async Task GetAuthenticatedCustomerTry_Should_Return_A_403Forbidden()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);

        // Act
        var response = await Client.GetAsync("trials/authenticated-customer");

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    #endregion
}
