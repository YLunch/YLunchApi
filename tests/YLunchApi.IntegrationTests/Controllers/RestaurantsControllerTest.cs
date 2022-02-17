using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.IntegrationTests.Core.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class RestaurantsControllerTest : ControllerTestBase
{
    [Fact]
    public async Task Post_Restaurant_Should_Return_A_201Created()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var body = new
        {
            RestaurantMocks.RestaurantCreateDto.Name,
            RestaurantMocks.RestaurantCreateDto.Email,
            RestaurantMocks.RestaurantCreateDto.PhoneNumber,
            RestaurantMocks.RestaurantCreateDto.Country,
            RestaurantMocks.RestaurantCreateDto.City,
            RestaurantMocks.RestaurantCreateDto.ZipCode,
            RestaurantMocks.RestaurantCreateDto.StreetName,
            RestaurantMocks.RestaurantCreateDto.StreetNumber,
            RestaurantMocks.RestaurantCreateDto.IsOpen,
            RestaurantMocks.RestaurantCreateDto.IsPublic,
            ClosingDates = new List<ClosingDateCreateDto>
            {
                new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
            },

            OpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    StartTimeInMinutes = 0,
                    EndTimeInMinutes = 1440,
                    StartOrderTimeInMinutes = 0,
                    EndOrderTimeInMinutes = 1440
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(response);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(authenticatedUserInfo.UserId);
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
        responseBody.ClosingDates.Should().BeEquivalentTo(body.ClosingDates);
        responseBody.OpeningTimes.Should().BeEquivalentTo(body.OpeningTimes);
        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);
        responseBody.IsPublished.Should().Be(true);
    }

    [Fact]
    public async Task Post_Restaurant_Should_Return_A_400BadRequest_When_Missing_Fields()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var body = new
        {
            Name = "",
            ClosingDates = new List<dynamic>()
            {
                new { ClosingDateTime = "" }
            },

            OpeningTimes = new List<dynamic>()
            {
                new
                {
                    DayOfWeek = 7,
                    StartOrderTimeInMinutes = 1440,
                    EndOrderTimeInMinutes = 1440
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("The Name field is required.");
        responseBody.Should().Contain("The Email field is required.");
        responseBody.Should().Contain("The PhoneNumber field is required.");
        responseBody.Should().Contain("The Country field is required.");
        responseBody.Should().Contain("The City field is required.");
        responseBody.Should().Contain("The ZipCode field is required.");
        responseBody.Should().Contain("The StreetName field is required.");
        responseBody.Should().Contain("The StreetNumber field is required.");
    }

    [Fact]
    public async Task Post_Restaurant_Should_Return_A_400BadRequest_When_Invalid_Fields()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var body = new
        {
            ClosingDates = new List<dynamic>()
            {
            },

            OpeningTimes = new List<dynamic>()
            {
                new
                {
                    DayOfWeek = 7,
                    StartOrderTimeInMinutes = 1440,
                    EndOrderTimeInMinutes = 1440
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("DayOfWeek\": [\"Day must be in range 0-6, 0 is sunday, 6 is saturday\"]");
    }

    [Fact]
    public async Task Post_Restaurant_Should_Return_A_401Unauthorized()
    {
        // Arrange
        var body = new
        {
            RestaurantMocks.RestaurantCreateDto.Name,
            RestaurantMocks.RestaurantCreateDto.Email,
            RestaurantMocks.RestaurantCreateDto.PhoneNumber,
            RestaurantMocks.RestaurantCreateDto.Country,
            RestaurantMocks.RestaurantCreateDto.City,
            RestaurantMocks.RestaurantCreateDto.ZipCode,
            RestaurantMocks.RestaurantCreateDto.StreetName,
            RestaurantMocks.RestaurantCreateDto.StreetNumber,
            RestaurantMocks.RestaurantCreateDto.IsOpen,
            RestaurantMocks.RestaurantCreateDto.IsPublic,
            ClosingDates = new List<ClosingDateCreateDto>
            {
                new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
            },

            OpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    StartTimeInMinutes = 0,
                    EndTimeInMinutes = 1440,
                    StartOrderTimeInMinutes = 0,
                    EndOrderTimeInMinutes = 1440
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("Please login and use provided tokens");
    }

    [Fact]
    public async Task Post_Restaurant_Should_Return_A_403Forbidden()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var body = new
        {
            RestaurantMocks.RestaurantCreateDto.Name,
            RestaurantMocks.RestaurantCreateDto.Email,
            RestaurantMocks.RestaurantCreateDto.PhoneNumber,
            RestaurantMocks.RestaurantCreateDto.Country,
            RestaurantMocks.RestaurantCreateDto.City,
            RestaurantMocks.RestaurantCreateDto.ZipCode,
            RestaurantMocks.RestaurantCreateDto.StreetName,
            RestaurantMocks.RestaurantCreateDto.StreetNumber,
            RestaurantMocks.RestaurantCreateDto.IsOpen,
            RestaurantMocks.RestaurantCreateDto.IsPublic,
            ClosingDates = new List<ClosingDateCreateDto>
            {
                new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
            },

            OpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    StartTimeInMinutes = 0,
                    EndTimeInMinutes = 1440,
                    StartOrderTimeInMinutes = 0,
                    EndOrderTimeInMinutes = 1440
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Be("");
    }
}
