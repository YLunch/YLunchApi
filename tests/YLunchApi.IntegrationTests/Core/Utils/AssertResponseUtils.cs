using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace YLunchApi.IntegrationTests.Core.Utils;

public static class AssertResponseUtils
{
    public static async Task AssertUnauthorizedResponse(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await ResponseUtils.DeserializeContentAsync(response);

        // Assert
        content.Should()
               .Contain("Please login and use provided tokens");
    }
}
