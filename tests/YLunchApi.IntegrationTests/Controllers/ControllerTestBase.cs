using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.IntegrationTests.Core;
using YLunchApi.IntegrationTests.Core.Utils;

namespace YLunchApi.IntegrationTests.Controllers;

public abstract class ControllerTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;

    protected ControllerTestBase()
    {
        var webApplication = new CustomWebApplicationFactory<Program>();
        Client = webApplication.CreateClient();
    }

    protected async Task<ApplicationSecurityToken> Authenticate(CustomerCreateDto customerCreateDto)
    {
        var customerCreationRequestBody = new
        {
            customerCreateDto.Email,
            customerCreateDto.Password,
            customerCreateDto.PhoneNumber,
            customerCreateDto.Lastname,
            customerCreateDto.Firstname
        };

        _ = await Client.PostAsJsonAsync("customers", customerCreationRequestBody);

        var applicationSecurityToken = await AuthenticateUser(customerCreateDto);
        applicationSecurityToken.UserRoles.Should().BeEquivalentTo(new List<string> { Roles.Customer });
        return applicationSecurityToken;
    }

    protected async Task<ApplicationSecurityToken> Authenticate(RestaurantAdminCreateDto restaurantAdminCreateDto)
    {
        var restaurantAdminCreationRequestBody = new
        {
            restaurantAdminCreateDto.Email,
            restaurantAdminCreateDto.Password,
            restaurantAdminCreateDto.PhoneNumber,
            restaurantAdminCreateDto.Lastname,
            restaurantAdminCreateDto.Firstname
        };

        _ = await Client.PostAsJsonAsync("restaurant-admins", restaurantAdminCreationRequestBody);

        var applicationSecurityToken = await AuthenticateUser(restaurantAdminCreateDto);
        applicationSecurityToken.UserRoles.Should().BeEquivalentTo(new List<string> { Roles.RestaurantAdmin });
        return applicationSecurityToken;
    }

    private async Task<ApplicationSecurityToken> AuthenticateUser(UserCreateDto userCreateDto)
    {
        // Arrange
        var userLoginRequestBody = new
        {
            email = userCreateDto.Email,
            userCreateDto.Password
        };

        // Act
        var loginResponse = await Client.PostAsJsonAsync("authentication/login", userLoginRequestBody);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await ResponseUtils.DeserializeContentAsync<TokenReadDto>(loginResponse);
        Assert.IsType<string>(tokens.AccessToken);
        Assert.IsType<string>(tokens.RefreshToken);
        var applicationSecurityToken = new ApplicationSecurityToken(tokens.AccessToken, tokens.RefreshToken);
        applicationSecurityToken.UserEmail.Should().Be(userCreateDto.Email);
        return applicationSecurityToken;
    }

    protected async Task AssertUnauthorizedResponse(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await ResponseUtils.DeserializeContentAsync(response);

        // Assert
        content.Should()
            .Contain("Please login and use provided tokens");
    }
}
