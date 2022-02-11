using System.Net.Http;
using System.Net.Http.Headers;

namespace YLunchApi.IntegrationTests.Core.Extensions;

public static class HttpClientExtension
{
    public static void SetAuthorizationHeader(this HttpClient httpClient, string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
