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
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.IntegrationTests.Core;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared.Models;

namespace YLunchApi.IntegrationTests.Controllers;

public abstract class ControllerITestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;

    protected ControllerITestBase()
    {
        var webApplication = new CustomWebApplicationFactory<Program>();
        Client = webApplication.CreateClient();
    }

    protected async Task<ApplicationSecurityTokenAndRefreshToken> Authenticate(CustomerCreateDto customerCreateDto)
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

        var decodedAccessToken = await AuthenticateUser(customerCreateDto);
        decodedAccessToken.UserRoles.Should().BeEquivalentTo(new List<string> { Roles.Customer });
        return decodedAccessToken;
    }

    protected async Task<ApplicationSecurityTokenAndRefreshToken> Authenticate(RestaurantAdminCreateDto restaurantAdminCreateDto)
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

        var decodedAccessToken = await AuthenticateUser(restaurantAdminCreateDto);
        decodedAccessToken.UserRoles.Should().BeEquivalentTo(new List<string> { Roles.RestaurantAdmin });
        return decodedAccessToken;
    }

    private async Task<ApplicationSecurityTokenAndRefreshToken> AuthenticateUser(UserCreateDto userCreateDto)
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
        var applicationSecurityToken = new ApplicationSecurityToken(tokens.AccessToken);
        applicationSecurityToken.UserEmail.Should().Be(userCreateDto.Email);
        return new ApplicationSecurityTokenAndRefreshToken(tokens.AccessToken, tokens.RefreshToken);
    }
}
