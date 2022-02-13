using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace YLunchApi.IntegrationTests.Core.Utils;

public static class ResponseUtils
{
    public static async Task<T> DeserializeContentAsync<T>(HttpResponseMessage response)
    {
        var jsonContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonContent, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    public static async Task<string> DeserializeContentAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }
}
