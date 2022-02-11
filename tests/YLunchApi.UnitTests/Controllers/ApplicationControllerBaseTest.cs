using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Main.Controllers;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class ApplicationControllerBaseTest
{
    [Fact]
    public void UserInfo_Should_Be_Unset_When_Http_Context_Is_Null()
    {
        // Arrange
        var httpContextAccessor = HttpContextAccessorMocker.GetWithoutAuthorization();

        // Act
        var controller = new TrialsController(httpContextAccessor);
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(
            $"YLunchApi is running, you are authenticated as {string.Empty} with Id: {string.Empty} and Roles: {string.Empty}");
    }

    [Fact]
    public void UserInfo_Should_Be_Unset_When_Authorization_Header_Value_Is_EmptyString()
    {
        // Arrange
        var httpContextAccessor = HttpContextAccessorMocker.GetWithAuthorization("");

        // Act
        var controller = new TrialsController(httpContextAccessor);
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(
            $"YLunchApi is running, you are authenticated as {string.Empty} with Id: {string.Empty} and Roles: {string.Empty}");
    }
}
