using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Authentication.Models.Dto;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Helpers.Extensions;
using YLunchApi.IntegrationTests.Core;
using YLunchApi.IntegrationTests.Core.Extensions;
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

    #region UserUtils

    protected async Task<UserReadDto> CreateUser(CustomerCreateDto customerCreateDto)
    {
        // Arrange
        var customerCreationRequestBody = new
        {
            customerCreateDto.Email,
            customerCreateDto.Password,
            customerCreateDto.PhoneNumber,
            customerCreateDto.Lastname,
            customerCreateDto.Firstname
        };

        // Act
        var response = await Client.PostAsJsonAsync("customers", customerCreationRequestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<UserReadDto>(response);
        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.Email.Should().Be(customerCreateDto.Email);
        responseBody.PhoneNumber.Should().Be(customerCreateDto.PhoneNumber);
        responseBody.Lastname.Should().Be(customerCreateDto.Lastname.Capitalize());
        responseBody.Firstname.Should().Be(customerCreateDto.Firstname.Capitalize());

        return responseBody;
    }

    protected async Task<UserReadDto> CreateUser(RestaurantAdminCreateDto restaurantAdminCreateDto)
    {
        var restaurantAdminCreationRequestBody = new
        {
            restaurantAdminCreateDto.Email,
            restaurantAdminCreateDto.Password,
            restaurantAdminCreateDto.PhoneNumber,
            restaurantAdminCreateDto.Lastname,
            restaurantAdminCreateDto.Firstname
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurant-admins", restaurantAdminCreationRequestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<UserReadDto>(response);
        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.Email.Should().Be(restaurantAdminCreateDto.Email);
        responseBody.PhoneNumber.Should().Be(restaurantAdminCreateDto.PhoneNumber);
        responseBody.Lastname.Should().Be(restaurantAdminCreateDto.Lastname.Capitalize());
        responseBody.Firstname.Should().Be(restaurantAdminCreateDto.Firstname.Capitalize());

        return responseBody;
    }

    private async Task<DecodedTokens> Login(UserCreateDto userCreateDto)
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
        return new DecodedTokens(tokens.AccessToken, tokens.RefreshToken);
    }

    protected async Task<DecodedTokens> CreateAndLogin(CustomerCreateDto customerCreateDto)
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

        var decodedTokens = await Login(customerCreateDto);
        decodedTokens.UserRoles.Should().BeEquivalentTo(new List<string> { Roles.Customer });
        return decodedTokens;
    }

    protected async Task<DecodedTokens> CreateAndLogin(RestaurantAdminCreateDto restaurantAdminCreateDto)
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

        var decodedTokens = await Login(restaurantAdminCreateDto);
        decodedTokens.UserRoles.Should().BeEquivalentTo(new List<string> { Roles.RestaurantAdmin });
        return decodedTokens;
    }

    #endregion

    #region RestaurantUtils

    protected async Task<RestaurantReadDto> CreateRestaurant(string accessToken, RestaurantCreateDto restaurantCreateDto)
    {
        // Arrange
        Client.SetAuthorizationHeader(accessToken);
        var body = new
        {
            restaurantCreateDto.Name,
            restaurantCreateDto.Email,
            restaurantCreateDto.PhoneNumber,
            restaurantCreateDto.Country,
            restaurantCreateDto.City,
            restaurantCreateDto.ZipCode,
            restaurantCreateDto.StreetName,
            restaurantCreateDto.StreetNumber,
            restaurantCreateDto.IsOpen,
            restaurantCreateDto.IsPublic,
            ClosingDates = restaurantCreateDto.ClosingDates
                                              .Select(x => x.Adapt<dynamic>())
                                              .ToList(),
            PlaceOpeningTimes = restaurantCreateDto.PlaceOpeningTimes
                                                   .Select(x => x.Adapt<dynamic>())
                                                   .ToList(),
            OrderOpeningTimes = restaurantCreateDto.OrderOpeningTimes
                                                   .Select(x => x.Adapt<dynamic>())
                                                   .ToList()
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(response);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(new ApplicationSecurityToken(accessToken).UserId);
        responseBody.Email.Should().Be(body.Email);
        responseBody.PhoneNumber.Should().Be(body.PhoneNumber);
        responseBody.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        responseBody.Name.Should().Be(body.Name);
        responseBody.City.Should().Be(body.City);
        responseBody.Country.Should().Be(body.Country);
        responseBody.StreetName.Should().Be(body.StreetName);
        responseBody.ZipCode.Should().Be(body.ZipCode);
        responseBody.StreetNumber.Should().Be(body.StreetNumber);
        responseBody.IsOpen.Should().Be(body.IsOpen);
        responseBody.IsPublic.Should().Be(body.IsPublic);

        responseBody.ClosingDates.Should().BeEquivalentTo(body.ClosingDates)
                    .And
                    .BeInAscendingOrder(x => x.ClosingDateTime);

        responseBody.PlaceOpeningTimes.Should().BeEquivalentTo(
            OpeningTimeUtils.AscendingOrder(body.PlaceOpeningTimes.Adapt<List<OpeningTimeCreateDto>>()),
            options => options.WithStrictOrdering());
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        Assert.IsType<bool>(responseBody.IsCurrentlyOpenInPlace);

        responseBody.OrderOpeningTimes.Should().BeEquivalentTo(
            OpeningTimeUtils.AscendingOrder(body.OrderOpeningTimes.Adapt<List<OpeningTimeCreateDto>>()),
            options => options.WithStrictOrdering());
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        Assert.IsType<bool>(responseBody.IsCurrentlyOpenToOrder);

        Assert.IsType<bool>(responseBody.IsPublished);

        return responseBody;
    }

    #endregion
}
