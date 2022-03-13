using Microsoft.AspNetCore.Http;
using Moq;

namespace YLunchApi.UnitTests.Core.Mocks;

public class HttpContextAccessorMock : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }

    public HttpContextAccessorMock(string? accessToken)
    {
        if (accessToken == null)
        {
            SetAnonymousHttpContext();
        }
        else
        {
            SetAuthenticatedHttpContext(accessToken);
        }
    }

    private void SetAnonymousHttpContext()
    {
        HttpContext = new Mock<IHttpContextAccessor>().Object.HttpContext;
    }

    private void SetAuthenticatedHttpContext(string accessToken)
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {accessToken}";
        httpContextAccessorMock.SetupProperty(x => x.HttpContext, httpContext);
        HttpContext = httpContextAccessorMock.Object.HttpContext;
    }
}
