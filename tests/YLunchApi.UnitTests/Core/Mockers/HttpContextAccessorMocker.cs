using Microsoft.AspNetCore.Http;
using Moq;

namespace YLunchApi.UnitTests.Core.Mockers;

public static class HttpContextAccessorMocker
{
    public static IHttpContextAccessor GetWithoutAuthorization()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        return httpContextAccessorMock.Object;
    }

    public static IHttpContextAccessor GetWithAuthorization(string accessToken)
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {accessToken}";
        httpContextAccessorMock.SetupProperty(x => x.HttpContext, httpContext);
        return httpContextAccessorMock.Object;
    }
}
