namespace YLunchApi.Domain.Core.Utils;

public static class EnvironmentUtils
{
    public static bool IsDevelopment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    public static string BaseUrl => IsDevelopment ? "http://localhost:5254" : "https://ylunch-api.rael-calitro.ovh";
}
