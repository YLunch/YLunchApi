using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.Main.Controllers;
using YLunchApi.UnitTests.Application.Authentication;

namespace YLunchApi.UnitTests.Controllers;

public class TrialsControllerTest
{
    private readonly TrialsController _trialsController;

    public TrialsControllerTest()
    {
        _trialsController = new TrialsController(new Mock<IHttpContextAccessor>().Object);
    }

    [Fact]
    public void GetAnonymousTry_Should_Return_A_200Ok()
    {
        // Act
        var response = _trialsController.GetAnonymousTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);

        responseBody.Should().Be("YLunchApi is running, you are anonymous");
    }

    [Fact]
    public void GetAuthenticatedTry_Should_Return_A_200Ok()
    {
        // Assert
        var authenticatedUserInfo = new ApplicationSecurityToken(TokenMocks.ValidAccessToken);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {TokenMocks.ValidAccessToken}";
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupProperty(x => x.HttpContext, httpContext);

        var controller = new TrialsController(httpContextAccessor.Object);

        // Act
        var response = controller.GetAuthenticatedTry();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(
            $"YLunchApi is running, you are authenticated as {authenticatedUserInfo.UserEmail} with Id: {authenticatedUserInfo.UserId} and Roles: {Roles.ListToString(authenticatedUserInfo.UserRoles)}");
    }
}
