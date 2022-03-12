using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Configuration;

namespace YLunchApi.UnitTests.Controllers;

public class TrialsControllerTest : IClassFixture<UnitTestFixture>
{
    private readonly UnitTestFixture _fixture;

    public TrialsControllerTest(UnitTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetAnonymousTry_Should_Return_A_200Ok()
    {
        // Arrange
        _fixture.InitFixture();
        var controller = _fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAnonymousTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new MessageDto("YLunchApi is running, you are anonymous."));
    }

    [Fact]
    public void GetAuthenticatedTry_Should_Return_A_200Ok()
    {
        // Arrange
        _fixture.InitFixture(configuration =>
            configuration.AccessToken = TokenMocks.ValidCustomerAccessToken);
        var controller = _fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        var authenticatedUserInfo = new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken);
        responseBody.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}."));
    }

    [Fact]
    public void GetAuthenticatedRestaurantAdminTry_Should_Return_A_200Ok()
    {
        // Arrange
        _fixture.InitFixture(configuration =>
            configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken);
        var controller = _fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAuthenticatedRestaurantAdminTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        var authenticatedUserInfo = new ApplicationSecurityToken(TokenMocks.ValidRestaurantAdminAccessToken);
        responseBody.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.RestaurantAdmin}."));
    }

    [Fact]
    public void GetAuthenticatedCustomerTry_Should_Return_A_200Ok()
    {
        // Arrange
        _fixture.InitFixture(configuration =>
            configuration.AccessToken = TokenMocks.ValidCustomerAccessToken);
        var controller = _fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAuthenticatedCustomerTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        var authenticatedUserInfo = new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken);
        responseBody.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.Customer}."));
    }
}
