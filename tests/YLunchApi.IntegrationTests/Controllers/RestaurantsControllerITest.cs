using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Mapster;
using Xunit;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.IntegrationTests.Core.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class RestaurantsControllerITest : ControllerITestBase
{
    #region Utils

    private async Task<RestaurantReadDto> CreateAndLoginRestaurantAdminAndCreateRestaurant(string restaurantAdminEmail, RestaurantCreateDto restaurantCreateDto)
    {
        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = restaurantAdminEmail;
        var decodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);
        return await CreateRestaurant(decodedTokens.AccessToken, restaurantCreateDto);
    }

    #endregion

    #region CreateRestaurantTests

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_201Created()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(decodedTokens.AccessToken);
        var utcNow = DateTime.UtcNow;
        var body = new
        {
            RestaurantMocks.SimpleRestaurantCreateDto.Name,
            RestaurantMocks.SimpleRestaurantCreateDto.Email,
            RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.Country,
            RestaurantMocks.SimpleRestaurantCreateDto.City,
            RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
            RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
            ClosingDates = new List<dynamic>
            {
                new { ClosingDateTime = DateTime.UtcNow.AddYears(1).AddDays(10) },
                new { ClosingDateTime = DateTime.UtcNow.AddYears(1).AddDays(-10) }
            },
            PlaceOpeningTimes = new List<dynamic>
            {
                new
                {
                    utcNow.AddDays(-1).DayOfWeek,
                    OffsetInMinutes = 0 * 60,
                    DurationInMinutes = 23 * 60 + 59
                },
                new
                {
                    utcNow.DayOfWeek,
                    OffsetInMinutes = 0 * 60,
                    DurationInMinutes = 23 * 60 + 59
                }
            },
            OrderOpeningTimes = new List<dynamic>
            {
                new
                {
                    utcNow.AddDays(-1).DayOfWeek,
                    OffsetInMinutes = 0 * 60,
                    DurationInMinutes = 23 * 60 + 59
                },
                new
                {
                    utcNow.DayOfWeek,
                    OffsetInMinutes = 0 * 60,
                    DurationInMinutes = 23 * 60 + 59
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(response);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(decodedTokens.UserId);
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
        responseBody.IsCurrentlyOpenInPlace.Should().Be(true);

        responseBody.OrderOpeningTimes.Should().BeEquivalentTo(
            OpeningTimeUtils.AscendingOrder(body.OrderOpeningTimes.Adapt<List<OpeningTimeCreateDto>>()),
            options => options.WithStrictOrdering());
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);

        responseBody.IsPublished.Should().Be(true);
    }

    [Fact]
    public async Task CreateRestaurant_Non_Having_Optional_Fields_Should_Return_A_201Created()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(decodedTokens.AccessToken);
        var body = new
        {
            RestaurantMocks.SimpleRestaurantCreateDto.Name,
            RestaurantMocks.SimpleRestaurantCreateDto.Email,
            RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.Country,
            RestaurantMocks.SimpleRestaurantCreateDto.City,
            RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
            RestaurantMocks.SimpleRestaurantCreateDto.IsPublic
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(response);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(decodedTokens.UserId);
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

        responseBody.IsPublished.Should().Be(false);
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Missing_Fields()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(decodedTokens.AccessToken);
        var body = new
        {
            Name = "",
            PlaceOpeningTimes = new List<dynamic>
            {
                new { }
            },
            OrderOpeningTimes = new List<dynamic>
            {
                new { }
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
        responseBody.Should().MatchRegex(@"PlaceOpeningTimes.*The DayOfWeek field is required\.");
        responseBody.Should()
                    .MatchRegex(
                        @"PlaceOpeningTimes.*The OffsetInMinutes field is required\.");
        responseBody.Should()
                    .MatchRegex(
                        @"PlaceOpeningTimes.*The DurationInMinutes field is required\.");

        responseBody.Should().MatchRegex(@"OrderOpeningTimes.*The DayOfWeek field is required\.");
        responseBody.Should()
                    .MatchRegex(
                        @"OrderOpeningTimes.*The OffsetInMinutes field is required\.");
        responseBody.Should()
                    .MatchRegex(
                        @"OrderOpeningTimes.*The DurationInMinutes field is required\.");
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Invalid_Fields()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(decodedTokens.AccessToken);
        var body = new
        {
            RestaurantMocks.SimpleRestaurantCreateDto.Name,
            Email = "bad email",
            PhoneNumber = "bad phone",
            RestaurantMocks.SimpleRestaurantCreateDto.Country,
            RestaurantMocks.SimpleRestaurantCreateDto.City,
            ZipCode = "bad zipcode",
            RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
            RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
            ClosingDates = new List<dynamic>
            {
                new { ClosingDateTime = DateTime.UtcNow.AddDays(10) },
                new { ClosingDateTime = DateTime.UtcNow.AddDays(-10) }
            },
            PlaceOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 7,
                    OffsetInMinutes = 24 * 60,
                    DurationInMinutes = 7 * 24 * 60
                }
            },
            OrderOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 7,
                    OffsetInMinutes = 24 * 60,
                    DurationInMinutes = 7 * 24 * 60
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

        responseBody.Should().MatchRegex(@"ClosingDates.*DateTime must be in future if present\.");
        responseBody.Should().MatchRegex(@"PlaceOpeningTimes.*Day must be in range 0-6, 0 is sunday, 6 is saturday\.");
        responseBody.Should()
                    .MatchRegex(
                        @"PlaceOpeningTimes.*OffsetInMinutes should be less than number of minutes in a day\.");
        responseBody.Should()
                    .MatchRegex(
                        @"PlaceOpeningTimes.*DurationInMinutes should be less than number of minutes in a week\.");

        responseBody.Should().MatchRegex(@"OrderOpeningTimes.*Day must be in range 0-6, 0 is sunday, 6 is saturday\.");
        responseBody.Should()
                    .MatchRegex(
                        @"OrderOpeningTimes.*OffsetInMinutes should be less than number of minutes in a day\.");
        responseBody.Should()
                    .MatchRegex(
                        @"OrderOpeningTimes.*DurationInMinutes should be less than number of minutes in a week\.");
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Overriding_Opening_Times()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(decodedTokens.AccessToken);
        var body = new
        {
            RestaurantMocks.SimpleRestaurantCreateDto.Name,
            RestaurantMocks.SimpleRestaurantCreateDto.Email,
            RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.Country,
            RestaurantMocks.SimpleRestaurantCreateDto.City,
            RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
            RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
            PlaceOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 1,
                    OffsetInMinutes = 2 * 60,
                    DurationInMinutes = 60
                },
                new
                {
                    DayOfWeek = 1,
                    OffsetInMinutes = 60,
                    DurationInMinutes = 120
                }
            },
            OrderOpeningTimes = new List<dynamic>
            {
                new
                {
                    DayOfWeek = 1,
                    OffsetInMinutes = 2 * 60,
                    DurationInMinutes = 60
                },
                new
                {
                    DayOfWeek = 1,
                    OffsetInMinutes = 60,
                    DurationInMinutes = 120
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
    public async Task CreateRestaurant_Should_Return_A_401Unauthorized()
    {
        // Arrange
        var body = new
        {
            RestaurantMocks.SimpleRestaurantCreateDto.Name,
            RestaurantMocks.SimpleRestaurantCreateDto.Email,
            RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.Country,
            RestaurantMocks.SimpleRestaurantCreateDto.City,
            RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
            RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
            ClosingDates = new List<ClosingDateCreateDto>
            {
                new() { ClosingDateTime = DateTime.UtcNow.AddYears(1).AddDays(10) }
            },
            PlaceOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetInMinutes = 0,
                    DurationInMinutes = 1439 //23H59
                }
            },
            OrderOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetInMinutes = 0,
                    DurationInMinutes = 1439 //23H59
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_403Forbidden()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(decodedTokens.AccessToken);
        var body = new
        {
            RestaurantMocks.SimpleRestaurantCreateDto.Name,
            RestaurantMocks.SimpleRestaurantCreateDto.Email,
            RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.Country,
            RestaurantMocks.SimpleRestaurantCreateDto.City,
            RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
            RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
            RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
            RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
            ClosingDates = new List<ClosingDateCreateDto>
            {
                new() { ClosingDateTime = DateTime.UtcNow.AddYears(1).AddDays(10) }
            },
            PlaceOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetInMinutes = 0,
                    DurationInMinutes = 1439 //23H59
                }
            },
            OrderOpeningTimes = new List<OpeningTimeCreateDto>
            {
                new()
                {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    OffsetInMinutes = 0,
                    DurationInMinutes = 1439 //23H59
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("restaurants", body);

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    #endregion

    #region GetRestaurantByIdTests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok()
    {
        // Arrange
        var restaurant = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            UserMocks.RestaurantAdminCreateDto.Email,
            RestaurantMocks.PrepareFullRestaurant("restaurant", DateTime.UtcNow)
        );

        // Act
        var response = await Client.GetAsync($"restaurants/{restaurant.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(response);

        responseBody.Id.Should().Be(restaurant.Id);
        responseBody.AdminId.Should().Be(restaurant.AdminId);
        responseBody.Email.Should().Be(restaurant.Email);
        responseBody.PhoneNumber.Should().Be(restaurant.PhoneNumber);
        responseBody.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        responseBody.Name.Should().Be(restaurant.Name);
        responseBody.City.Should().Be(restaurant.City);
        responseBody.Country.Should().Be(restaurant.Country);
        responseBody.StreetName.Should().Be(restaurant.StreetName);
        responseBody.ZipCode.Should().Be(restaurant.ZipCode);
        responseBody.StreetNumber.Should().Be(restaurant.StreetNumber);
        responseBody.IsOpen.Should().Be(restaurant.IsOpen);
        responseBody.IsPublic.Should().Be(restaurant.IsPublic);

        responseBody.ClosingDates.Should().BeEquivalentTo(restaurant.ClosingDates)
                    .And
                    .BeInAscendingOrder(x => x.ClosingDateTime);

        responseBody.PlaceOpeningTimes.Should().BeEquivalentTo(
            OpeningTimeUtils.AscendingOrder(restaurant.PlaceOpeningTimes.Adapt<List<OpeningTimeCreateDto>>()),
            options => options.WithStrictOrdering());
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenInPlace.Should().Be(true);

        responseBody.OrderOpeningTimes.Should().BeEquivalentTo(
            OpeningTimeUtils.AscendingOrder(restaurant.OrderOpeningTimes.Adapt<List<OpeningTimeCreateDto>>()),
            options => options.WithStrictOrdering());
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);

        responseBody.IsPublished.Should().Be(true);
    }

    #endregion

    #region GetRestaurantsTests

    [Fact]
    public async Task GetRestaurants_Should_Return_A_200Ok_Containing_Restaurants()
    {
        // Arrange
        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        var restaurant1 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"1-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto1
        );

        var restaurant2 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"2-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant2", DateTime.UtcNow)
        );

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        var restaurant3 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"3-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto3
        );

        var restaurant4 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"4-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant4", DateTime.UtcNow)
        );

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        var restaurant5 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"5-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto5
        );

        var restaurant6 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"6-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant6", DateTime.UtcNow)
        );

        var restaurant7 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"7-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant7", DateTime.UtcNow)
        );

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant1,
            restaurant2,
            restaurant3,
            restaurant4,
            restaurant5,
            restaurant6,
            restaurant7
        };

        // Act
        var response = await Client.GetAsync("restaurants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<RestaurantReadDto>>(response);
        responseBody.Count.Should().Be(expectedRestaurants.Count);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().Be(expectedRestaurant.Id);
            actualRestaurant.AdminId.Should().Be(expectedRestaurant.AdminId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(expectedRestaurant.IsCurrentlyOpenInPlace);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(expectedRestaurant.IsCurrentlyOpenToOrder);
            actualRestaurant.IsPublished.Should().Be(expectedRestaurant.IsPublished);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_Should_Return_A_200Ok_Containing_Paginated_Restaurants()
    {
        // Arrange
        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"1-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto1
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"2-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant2", DateTime.UtcNow)
        );

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"3-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto3
        );

        var restaurant4 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"4-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant4", DateTime.UtcNow)
        );

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        var restaurant5 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"5-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto5
        );

        var restaurant6 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"6-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant6", DateTime.UtcNow)
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"7-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant7", DateTime.UtcNow)
        );

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant4,
            restaurant5,
            restaurant6
        };

        // Act
        var response = await Client.GetAsync("restaurants?page=2&size=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<RestaurantReadDto>>(response);
        responseBody.Count.Should().Be(expectedRestaurants.Count);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().Be(expectedRestaurant.Id);
            actualRestaurant.AdminId.Should().Be(expectedRestaurant.AdminId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(expectedRestaurant.IsCurrentlyOpenInPlace);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(expectedRestaurant.IsCurrentlyOpenToOrder);
            actualRestaurant.IsPublished.Should().Be(expectedRestaurant.IsPublished);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_Should_Return_A_200Ok_Containing_Restaurants_Which_Are_CurrentlyOpenToOrder()
    {
        // Arrange
        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"1-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto1
        );

        var restaurant2 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"2-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant2", DateTime.UtcNow)
        );

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"3-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto3
        );

        var restaurant4 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"4-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant4", DateTime.UtcNow)
        );

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"5-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto5
        );

        var restaurant6 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"6-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant6", DateTime.UtcNow)
        );

        var restaurant7 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"7-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant7", DateTime.UtcNow)
        );

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant2,
            restaurant4,
            restaurant6,
            restaurant7
        };

        // Act
        var response = await Client.GetAsync("restaurants?isCurrentlyOpenToOrder=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<RestaurantReadDto>>(response);
        responseBody.Count.Should().Be(expectedRestaurants.Count);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(expectedRestaurant.AdminId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(true);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(true);
            actualRestaurant.IsPublished.Should().Be(true);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_Should_Return_A_200Ok_Containing_Restaurants_Which_Are_Not_CurrentlyOpenToOrder()
    {
        // Arrange
        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        var restaurant1 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"1-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto1
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"2-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant2", DateTime.UtcNow)
        );

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        var restaurant3 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"3-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto3
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"4-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant4", DateTime.UtcNow)
        );

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        var restaurant5 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"5-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto5
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"6-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant6", DateTime.UtcNow)
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"7-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant7", DateTime.UtcNow)
        );

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant1,
            restaurant3,
            restaurant5
        };

        // Act
        var response = await Client.GetAsync("restaurants?isCurrentlyOpenToOrder=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<RestaurantReadDto>>(response);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(expectedRestaurant.AdminId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.OrderOpeningTimes.Should().BeEmpty();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(false);
            actualRestaurant.IsPublished.Should().Be(false);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_Should_Return_A_200Ok_Containing_Published_Restaurants()
    {
        // Arrange
        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"1-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto1
        );

        var restaurant2 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"2-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant2", DateTime.UtcNow)
        );

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"3-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto3
        );

        var restaurant4 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"4-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant4", DateTime.UtcNow)
        );

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"5-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto5
        );

        var restaurant6 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"6-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant6", DateTime.UtcNow)
        );

        var restaurant7 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"7-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant7", DateTime.UtcNow)
        );

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant2,
            restaurant4,
            restaurant6,
            restaurant7
        };

        // Act
        var response = await Client.GetAsync("restaurants?isPublished=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<RestaurantReadDto>>(response);
        responseBody.Count.Should().Be(expectedRestaurants.Count);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(expectedRestaurant.AdminId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(true);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(true);
            actualRestaurant.IsPublished.Should().Be(true);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_Should_Return_A_200Ok_Containing_Not_Published_Restaurants()
    {
        // Arrange
        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        var restaurant1 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"1-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto1
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"2-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant2", DateTime.UtcNow)
        );

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        var restaurant3 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"3-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto3
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"4-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant4", DateTime.UtcNow)
        );

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        var restaurant5 = await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"5-{UserMocks.RestaurantAdminCreateDto.Email}",
            restaurantCreateDto5
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"6-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant6", DateTime.UtcNow)
        );

        await CreateAndLoginRestaurantAdminAndCreateRestaurant(
            $"7-{UserMocks.RestaurantAdminCreateDto.Email}",
            RestaurantMocks.PrepareFullRestaurant("restaurant7", DateTime.UtcNow)
        );

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant1,
            restaurant3,
            restaurant5
        };

        // Act
        var response = await Client.GetAsync("restaurants?isPublished=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<RestaurantReadDto>>(response);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(expectedRestaurant.AdminId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.OrderOpeningTimes.Should().BeEmpty();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(false);
            actualRestaurant.IsPublished.Should().Be(false);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_Should_Return_A_400BadRequest_When_Invalid_Filter()
    {
        // Arrange & Act
        var response = await Client.GetAsync("restaurants?page=0&size=31&isPublished=25&isCurrentlyOpenToOrder=abc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("Page must be an integer between 1 and 100000.");
        responseBody.Should().Contain("Size must be an integer between 1 and 30.");
        responseBody.Should().Contain("The value '25' is not valid for IsPublished.");
        responseBody.Should().Contain("The value 'abc' is not valid for IsCurrentlyOpenToOrder.");
    }

    #endregion
}
