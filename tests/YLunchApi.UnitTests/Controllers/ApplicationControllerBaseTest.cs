using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Controllers;

public class ApplicationControllerBaseTest : UnitTestFixture
{
    public ApplicationControllerBaseTest(UnitTestFixtureBase fixture) : base(fixture)
    {
    }

    [Fact]
    public void UserInfo_Should_Be_Unset_When_Http_Context_Is_Null()
    {
        // Arrange
        Fixture.InitFixture();
        var controller = Fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {string.Empty} with Id: {string.Empty} and Roles: {string.Empty}."));
    }

    [Fact]
    public void UserInfo_Should_Be_Unset_When_Authorization_Header_Value_Is_EmptyString()
    {
        // Arrange
        Fixture.InitFixture(configuration => configuration.AccessToken = "");
        var controller = Fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {string.Empty} with Id: {string.Empty} and Roles: {string.Empty}."));
    }

    [Fact]
    public void UserInfo_Should_Be_Set_When_Authorization_Header_Value_Is_Set_With_A_Valid_Customer_AccessToken()
    {
        // Arrange
        const string accessToken = TokenMocks.ValidCustomerAccessToken;
        Fixture.InitFixture(configuration => configuration.AccessToken = accessToken);
        var controller = Fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        var applicationSecurityToken = new ApplicationSecurityToken(accessToken);

        responseBody.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {applicationSecurityToken.UserEmail} with Id: {applicationSecurityToken.UserId} and Roles: {Roles.ListToString(applicationSecurityToken.UserRoles)}."));
    }

    [Fact]
    public void UserInfo_Should_Be_Set_When_Authorization_Header_Value_Is_Set_With_A_Valid_RestaurantAdmin_AccessToken()
    {
        // Arrange
        const string accessToken = TokenMocks.ValidRestaurantAdminAccessToken;
        Fixture.InitFixture(configuration => configuration.AccessToken = accessToken);
        var controller = Fixture.GetImplementationFromService<TrialsController>();

        // Act
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        var applicationSecurityToken = new ApplicationSecurityToken(accessToken);

        responseBody.Should().BeEquivalentTo(new MessageDto(
            $"YLunchApi is running, you are authenticated as {applicationSecurityToken.UserEmail} with Id: {applicationSecurityToken.UserId} and Roles: {Roles.ListToString(applicationSecurityToken.UserRoles)}."));
    }
}
