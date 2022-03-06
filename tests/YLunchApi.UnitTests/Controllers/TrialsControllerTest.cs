using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class TrialsControllerTest
{
    [Fact]
    public void GetAnonymousTry_Should_Return_A_200Ok()
    {
        // Arrange
        var controller = new TrialsController(HttpContextAccessorMocker.GetWithoutAuthorization());

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
        var controller =
            new TrialsController(HttpContextAccessorMocker.GetWithAuthorization(TokenMocks.ValidCustomerAccessToken));

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
        var controller =
            new TrialsController(
                HttpContextAccessorMocker.GetWithAuthorization(TokenMocks.ValidRestaurantAdminAccessToken));

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
        var controller =
            new TrialsController(HttpContextAccessorMocker.GetWithAuthorization(TokenMocks.ValidCustomerAccessToken));

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
