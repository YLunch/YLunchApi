using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Helpers.Extensions;
using YLunchApi.IntegrationTests.Core.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class RestaurantsControllerTest : ControllerTestBase
{
    #region Post_Restaurant_Tests

    [Fact]
    public async Task Post_Restaurant_Should_Return_A_201Created()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var utcNow = DateTime.UtcNow;
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
            PlaceOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = utcNow.DayOfWeek,
                    OffsetOpenMinutes = utcNow.MinutesFromMidnight(),
                    OpenMinutes = 2 * 60
                }
            },
            OrderOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = utcNow.DayOfWeek,
                    OffsetOpenMinutes = utcNow.MinutesFromMidnight(),
                    OpenMinutes = 2 * 60
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

        responseBody.PlaceOpeningTimes.Should().BeEquivalentTo(body.PlaceOpeningTimes);
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenInPlace.Should().Be(true);

        responseBody.OrderOpeningTimes.Should().BeEquivalentTo(body.OrderOpeningTimes);
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
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
            Name = ""
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
            RestaurantMocks.RestaurantCreateDto.Name,
            Email = "bad email",
            PhoneNumber = "bad phone",
            RestaurantMocks.RestaurantCreateDto.Country,
            RestaurantMocks.RestaurantCreateDto.City,
            ZipCode = "bad zipcode",
            RestaurantMocks.RestaurantCreateDto.StreetName,
            RestaurantMocks.RestaurantCreateDto.StreetNumber,
            RestaurantMocks.RestaurantCreateDto.IsOpen,
            RestaurantMocks.RestaurantCreateDto.IsPublic,
            ClosingDates = new List<dynamic>
            {
                new {}
            },
            PlaceOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 7,
                    OffsetOpenMinutes = 24 * 60,
                    OpenMinutes = 7 * 24 * 60
                }
            },
            OrderOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 7,
                    OffsetOpenMinutes = 24 * 60,
                    OpenMinutes = 7 * 24 * 60
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("PhoneNumber is invalid. Example: '0612345678'.");
        responseBody.Should().Contain("ZipCode is invalid. Example: '06560'.");
        responseBody.Should()
                    .Contain("Email is invalid. It should be lowercase email format. Example: example@example.com.");

        responseBody.Should().MatchRegex(@"PlaceOpeningTimes.*Day must be in range 0-6, 0 is sunday, 6 is saturday\.");
        responseBody.Should()
                    .MatchRegex(
                        @"PlaceOpeningTimes.*OffsetOpenMinutes should be less than number of minutes in a day\.");
        responseBody.Should()
                    .MatchRegex(@"PlaceOpeningTimes.*OpenMinutes should be less than number of minutes in a week\.");

        responseBody.Should().MatchRegex(@"OrderOpeningTimes.*Day must be in range 0-6, 0 is sunday, 6 is saturday\.");
        responseBody.Should()
                    .MatchRegex(
                        @"OrderOpeningTimes.*OffsetOpenMinutes should be less than number of minutes in a day\.");
        responseBody.Should()
                    .MatchRegex(@"OrderOpeningTimes.*OpenMinutes should be less than number of minutes in a week\.");
    }

        [Fact]
    public async Task Post_Restaurant_Should_Return_A_400BadRequest_When_Overriding_Opening_Times()
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
            PlaceOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 1,
                    OffsetOpenMinutes = 2 * 60,
                    OpenMinutes = 60
                },
                new
                {
                    DayOfWeek = 1,
                    OffsetOpenMinutes = 60,
                    OpenMinutes = 120
                }
            },
            OrderOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 1,
                    OffsetOpenMinutes = 2 * 60,
                    OpenMinutes = 60
                },
                new
                {
                    DayOfWeek = 1,
                    OffsetOpenMinutes = 60,
                    OpenMinutes = 120
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);


        responseBody.Should()
                    .MatchRegex(
                        @"PlaceOpeningTimes.*Some opening times override others\.");

        responseBody.Should()
                    .MatchRegex(@"OrderOpeningTimes.*Some opening times override others\.");
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
            PlaceOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetOpenMinutes = 0,
                    OpenMinutes = 1439 //23H59
                }
            },
            OrderOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetOpenMinutes = 0,
                    OpenMinutes = 1439 //23H59
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
            PlaceOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetOpenMinutes = 0,
                    OpenMinutes = 1439 //23H59
                }
            },
            OrderOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetOpenMinutes = 0,
                    OpenMinutes = 1439 //23H59
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

    #endregion
}
