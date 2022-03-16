using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using YLunchApi.Domain.CommonAggregate.Dto;

namespace YLunchApi.IntegrationTests.Core.Utils;

public static class AssertResponseUtils
{
    public static async Task AssertUnauthorizedResponse(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseBody = await ResponseUtils.DeserializeContentAsync<ErrorDto>(response);

        // Assert
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.Unauthorized, "Please login and use provided tokens."));
    }

    public static async Task AssertForbiddenResponse(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var responseBody = await ResponseUtils.DeserializeContentAsync<ErrorDto>(response);

        // Assert
        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.Forbidden, "User has not granted roles."));
    }
}
