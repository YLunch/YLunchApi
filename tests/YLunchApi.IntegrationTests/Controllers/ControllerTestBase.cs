using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using YLunchApi.IntegrationTests.Core;

namespace YLunchApi.IntegrationTests.Controllers;

public class ControllerTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory<Program> WebApplication;

    protected ControllerTestBase()
    {
        WebApplication = new CustomWebApplicationFactory<Program>();

        // Client = new HttpClient(); //NOSONAR
        // Client.BaseAddress = new Uri("https://ylunch-api.rael-calitro.ovh/"); //NOSONAR

        Client = WebApplication.CreateClient();
    }
}
