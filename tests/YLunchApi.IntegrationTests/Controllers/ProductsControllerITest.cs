using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
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
public class ProductsControllerITest : ControllerITestBase
{
    #region Utils

    private async Task<RestaurantReadDto> CreateSimpleRestaurant(RestaurantCreateDto restaurantCreateDto)
    {
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var restaurantCreationResponse = await Client.PostAsJsonAsync("restaurants", restaurantCreateDto);
        restaurantCreationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(restaurantCreationResponse);
    }

    #endregion

    #region CreateRestaurant_Tests

    [Fact]
    public async Task CreateProduct_Should_Return_A_201Created()
    {
        // Arrange
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var restaurant = await CreateSimpleRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);
        var dateTime = DateTime.UtcNow;
        var productCreateDto = ProductMocks.ProductCreateDto;
        var body = new
        {
            productCreateDto.Name,
            productCreateDto.Price,
            productCreateDto.Quantity,
            productCreateDto.IsActive,
            productCreateDto.ProductType,
            ExpirationDateTime = dateTime.AddDays(1),
            productCreateDto.Description,
            productCreateDto.Allergens,
            productCreateDto.ProductTags
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await ResponseUtils.DeserializeContentAsync<ProductReadDto>(response);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.Name.Should().Be(body.Name);
        responseBody.Price.Should().Be(body.Price);
        responseBody.Description.Should().Be(body.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(body.Quantity);
        responseBody.ProductType.Should().Be(body.ProductType);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeCloseTo(dateTime.AddDays(1), TimeSpan.FromSeconds(5));
        responseBody.Allergens.Should().BeEquivalentTo(body.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.Allergens.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.ProductTags.Should().BeEquivalentTo(body.ProductTags)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.ProductTags.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
    }

    // [Fact]
    // public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Missing_Fields()
    // {
    //     // Arrange
    //     var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
    //     Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
    //     var body = new
    //     {
    //         Name = "",
    //         PlaceOpeningTimes = new List<dynamic>
    //         {
    //             new { }
    //         },
    //         OrderOpeningTimes = new List<dynamic>
    //         {
    //             new { }
    //         }
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("restaurants", body);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var responseBody = await ResponseUtils.DeserializeContentAsync(response);
    //
    //     responseBody.Should().Contain("The Name field is required.");
    //     responseBody.Should().Contain("The Email field is required.");
    //     responseBody.Should().Contain("The PhoneNumber field is required.");
    //     responseBody.Should().Contain("The Country field is required.");
    //     responseBody.Should().Contain("The City field is required.");
    //     responseBody.Should().Contain("The ZipCode field is required.");
    //     responseBody.Should().Contain("The StreetName field is required.");
    //     responseBody.Should().Contain("The StreetNumber field is required.");
    //     responseBody.Should().MatchRegex(@"PlaceOpeningTimes.*The DayOfWeek field is required\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"PlaceOpeningTimes.*The OffsetInMinutes field is required\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"PlaceOpeningTimes.*The DurationInMinutes field is required\.");
    //
    //     responseBody.Should().MatchRegex(@"OrderOpeningTimes.*The DayOfWeek field is required\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"OrderOpeningTimes.*The OffsetInMinutes field is required\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"OrderOpeningTimes.*The DurationInMinutes field is required\.");
    // }
    //
    // [Fact]
    // public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Invalid_Fields()
    // {
    //     // Arrange
    //     var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
    //     Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
    //     var body = new
    //     {
    //         RestaurantMocks.SimpleRestaurantCreateDto.Name,
    //         Email = "bad email",
    //         PhoneNumber = "bad phone",
    //         RestaurantMocks.SimpleRestaurantCreateDto.Country,
    //         RestaurantMocks.SimpleRestaurantCreateDto.City,
    //         ZipCode = "bad zipcode",
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
    //         ClosingDates = new List<dynamic>
    //         {
    //             new { }
    //         },
    //         PlaceOpeningTimes = new List<dynamic>
    //         {
    //             new
    //             {
    //                 DayOfWeek = 7,
    //                 OffsetInMinutes = 24 * 60,
    //                 DurationInMinutes = 7 * 24 * 60
    //             }
    //         },
    //         OrderOpeningTimes = new List<dynamic>
    //         {
    //             new
    //             {
    //                 DayOfWeek = 7,
    //                 OffsetInMinutes = 24 * 60,
    //                 DurationInMinutes = 7 * 24 * 60
    //             }
    //         }
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("restaurants", body);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var responseBody = await ResponseUtils.DeserializeContentAsync(response);
    //
    //     responseBody.Should().Contain("PhoneNumber is invalid. Example: '0612345678'.");
    //     responseBody.Should().Contain("ZipCode is invalid. Example: '06560'.");
    //     responseBody.Should()
    //                 .Contain("Email is invalid. It should be lowercase email format. Example: example@example.com.");
    //
    //     responseBody.Should().MatchRegex(@"PlaceOpeningTimes.*Day must be in range 0-6, 0 is sunday, 6 is saturday\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"PlaceOpeningTimes.*OffsetInMinutes should be less than number of minutes in a day\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"PlaceOpeningTimes.*DurationInMinutes should be less than number of minutes in a week\.");
    //
    //     responseBody.Should().MatchRegex(@"OrderOpeningTimes.*Day must be in range 0-6, 0 is sunday, 6 is saturday\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"OrderOpeningTimes.*OffsetInMinutes should be less than number of minutes in a day\.");
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"OrderOpeningTimes.*DurationInMinutes should be less than number of minutes in a week\.");
    // }
    //
    // [Fact]
    // public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Overriding_Opening_Times()
    // {
    //     // Arrange
    //     var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
    //     Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
    //     var body = new
    //     {
    //         RestaurantMocks.SimpleRestaurantCreateDto.Name,
    //         RestaurantMocks.SimpleRestaurantCreateDto.Email,
    //         RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
    //         RestaurantMocks.SimpleRestaurantCreateDto.Country,
    //         RestaurantMocks.SimpleRestaurantCreateDto.City,
    //         RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
    //         PlaceOpeningTimes = new List<dynamic>
    //         {
    //             new
    //             {
    //                 DayOfWeek = 1,
    //                 OffsetInMinutes = 2 * 60,
    //                 DurationInMinutes = 60
    //             },
    //             new
    //             {
    //                 DayOfWeek = 1,
    //                 OffsetInMinutes = 60,
    //                 DurationInMinutes = 120
    //             }
    //         },
    //         OrderOpeningTimes = new List<dynamic>
    //         {
    //             new
    //             {
    //                 DayOfWeek = 1,
    //                 OffsetInMinutes = 2 * 60,
    //                 DurationInMinutes = 60
    //             },
    //             new
    //             {
    //                 DayOfWeek = 1,
    //                 OffsetInMinutes = 60,
    //                 DurationInMinutes = 120
    //             }
    //         }
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("restaurants", body);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var responseBody = await ResponseUtils.DeserializeContentAsync(response);
    //
    //
    //     responseBody.Should()
    //                 .MatchRegex(
    //                     @"PlaceOpeningTimes.*Some opening times override others\.");
    //
    //     responseBody.Should()
    //                 .MatchRegex(@"OrderOpeningTimes.*Some opening times override others\.");
    // }
    //
    // [Fact]
    // public async Task CreateRestaurant_Should_Return_A_401Unauthorized()
    // {
    //     // Arrange
    //     var body = new
    //     {
    //         RestaurantMocks.SimpleRestaurantCreateDto.Name,
    //         RestaurantMocks.SimpleRestaurantCreateDto.Email,
    //         RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
    //         RestaurantMocks.SimpleRestaurantCreateDto.Country,
    //         RestaurantMocks.SimpleRestaurantCreateDto.City,
    //         RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
    //         ClosingDates = new List<ClosingDateCreateDto>
    //         {
    //             new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
    //         },
    //         PlaceOpeningTimes = new List<OpeningTimeCreateDto>
    //         {
    //             new()
    //             {
    //                 DayOfWeek = DateTime.UtcNow.DayOfWeek,
    //                 OffsetInMinutes = 0,
    //                 DurationInMinutes = 1439 //23H59
    //             }
    //         },
    //         OrderOpeningTimes = new List<OpeningTimeCreateDto>
    //         {
    //             new()
    //             {
    //                 DayOfWeek = DateTime.UtcNow.DayOfWeek,
    //                 OffsetInMinutes = 0,
    //                 DurationInMinutes = 1439 //23H59
    //             }
    //         }
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("restaurants", body);
    //
    //     // Assert
    //     await AssertResponseUtils.AssertUnauthorizedResponse(response);
    // }
    //
    // [Fact]
    // public async Task CreateRestaurant_Should_Return_A_403Forbidden()
    // {
    //     // Arrange
    //     var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
    //     Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
    //     var body = new
    //     {
    //         RestaurantMocks.SimpleRestaurantCreateDto.Name,
    //         RestaurantMocks.SimpleRestaurantCreateDto.Email,
    //         RestaurantMocks.SimpleRestaurantCreateDto.PhoneNumber,
    //         RestaurantMocks.SimpleRestaurantCreateDto.Country,
    //         RestaurantMocks.SimpleRestaurantCreateDto.City,
    //         RestaurantMocks.SimpleRestaurantCreateDto.ZipCode,
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetName,
    //         RestaurantMocks.SimpleRestaurantCreateDto.StreetNumber,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsOpen,
    //         RestaurantMocks.SimpleRestaurantCreateDto.IsPublic,
    //         ClosingDates = new List<ClosingDateCreateDto>
    //         {
    //             new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
    //         },
    //         PlaceOpeningTimes = new List<OpeningTimeCreateDto>
    //         {
    //             new()
    //             {
    //                 DayOfWeek = DateTime.UtcNow.DayOfWeek,
    //                 OffsetInMinutes = 0,
    //                 DurationInMinutes = 1439 //23H59
    //             }
    //         },
    //         OrderOpeningTimes = new List<OpeningTimeCreateDto>
    //         {
    //             new()
    //             {
    //                 DayOfWeek = DateTime.UtcNow.DayOfWeek,
    //                 OffsetInMinutes = 0,
    //                 DurationInMinutes = 1439 //23H59
    //             }
    //         }
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("restaurants", body);
    //
    //     // Assert
    //     await AssertResponseUtils.AssertForbiddenResponse(response);
    // }

    #endregion

    // #region GetRestaurantById_Tests
    //
    // [Fact]
    // public async Task GetRestaurantById_Should_Return_A_200Ok()
    // {
    //     // Arrange
    //     var restaurant = await CreateFullRestaurant();
    //
    //     // Act
    //     var response = await Client.GetAsync($"restaurants/{restaurant.Id}");
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.OK);
    //     var responseBody = await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(response);
    //
    //     responseBody.Id.Should().Be(restaurant.Id);
    //     responseBody.AdminId.Should().Be(restaurant.AdminId);
    //     responseBody.Email.Should().Be(restaurant.Email);
    //     responseBody.PhoneNumber.Should().Be(restaurant.PhoneNumber);
    //     responseBody.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    //     responseBody.Name.Should().Be(restaurant.Name);
    //     responseBody.City.Should().Be(restaurant.City);
    //     responseBody.Country.Should().Be(restaurant.Country);
    //     responseBody.StreetName.Should().Be(restaurant.StreetName);
    //     responseBody.ZipCode.Should().Be(restaurant.ZipCode);
    //     responseBody.StreetNumber.Should().Be(restaurant.StreetNumber);
    //     responseBody.IsOpen.Should().Be(restaurant.IsOpen);
    //     responseBody.IsPublic.Should().Be(restaurant.IsPublic);
    //
    //     responseBody.ClosingDates.Should().BeEquivalentTo(restaurant.ClosingDates)
    //                 .And
    //                 .BeInAscendingOrder(x => x.ClosingDateTime);
    //
    //     responseBody.PlaceOpeningTimes.Should().BeEquivalentTo(
    //         OpeningTimeUtils.AscendingOrder(restaurant.PlaceOpeningTimes.Adapt<List<OpeningTimeCreateDto>>()),
    //         options => options.WithStrictOrdering());
    //     responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
    //                 .Should().BeTrue();
    //     responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
    //                 .Should().BeTrue();
    //     responseBody.IsCurrentlyOpenInPlace.Should().Be(true);
    //
    //     responseBody.OrderOpeningTimes.Should().BeEquivalentTo(
    //         OpeningTimeUtils.AscendingOrder(restaurant.OrderOpeningTimes.Adapt<List<OpeningTimeCreateDto>>()),
    //         options => options.WithStrictOrdering());
    //     responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
    //                 .Should().BeTrue();
    //     responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
    //                 .Should().BeTrue();
    //     responseBody.IsCurrentlyOpenToOrder.Should().Be(true);
    //
    //     responseBody.IsPublished.Should().Be(true);
    // }
    //
    // #endregion
}
