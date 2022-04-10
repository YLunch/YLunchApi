using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.IntegrationTests.Core.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class OrdersControllerITest : ControllerITestBase
{
    #region CreateOrderTests

    [Fact]
    public async Task CreateOrder_Should_Return_A_201Created()
    {
        // Arrange
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, RestaurantMocks.PrepareFullRestaurant("restaurant", DateTime.UtcNow));

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        // Act & Assert
        _ = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto> { product1, product2, product3 });
    }

    // [Fact]
    // public async Task CreateOrder_Should_Return_A_400BadRequest_When_Missing_Fields()
    // {
    //     // Arrange
    //     var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
    //     var restaurant = await CreateRestaurant(decodedTokens.AccessToken, RestaurantMocks.SimpleRestaurantCreateDto);
    //     var body = new { };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var responseBody = await ResponseUtils.DeserializeContentAsync(response);
    //
    //     responseBody.Should().Contain("The Name field is required.");
    //     responseBody.Should().Contain("The Price field is required.");
    //     responseBody.Should().Contain("The IsActive field is required.");
    //     responseBody.Should().Contain("The Allergens field is required.");
    //     responseBody.Should().Contain("The ProductType field is required.");
    // }
    //
    // [Fact]
    // public async Task CreateOrder_Should_Return_A_400BadRequest_When_Invalid_Fields()
    // {
    //     // Arrange
    //     var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
    //     var restaurant = await CreateRestaurant(decodedTokens.AccessToken, RestaurantMocks.SimpleRestaurantCreateDto);
    //     var body = new
    //     {
    //         Name = "An invalid Name",
    //         Description = "An invalid Description",
    //         Quantity = 0,
    //         ExpirationDateTime = DateTime.UtcNow.AddDays(-1),
    //         Allergens = new List<dynamic>
    //         {
    //             new { BadFieldName = "wrong value" }
    //         },
    //         ProductTags = new List<dynamic>
    //         {
    //             new { BadFieldName = "wrong value" }
    //         }
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var responseBody = await ResponseUtils.DeserializeContentAsync(response);
    //
    //     responseBody.Should().MatchRegex(@"Name.*Must be lowercase\.");
    //     responseBody.Should().MatchRegex(@"Description.*Must be lowercase\.");
    //     responseBody.Should().MatchRegex(@"Quantity.*The field Quantity must be between 1 and 10000\.");
    //     responseBody.Should().MatchRegex(@"Allergens.*The Name field is required\.");
    //     responseBody.Should().MatchRegex(@"ProductTags.*The Name field is required\.");
    //     responseBody.Should().MatchRegex(@"ExpirationDateTime.*The Name field is required\.");
    // }
    //
    // [Fact]
    // public async Task CreateOrder_Should_Return_A_400BadRequest_When_Allergens_And_ProductTags_Are_Invalid()
    // {
    //     // Arrange
    //     var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
    //     var restaurant = await CreateRestaurant(decodedTokens.AccessToken, RestaurantMocks.SimpleRestaurantCreateDto);
    //     var body = new
    //     {
    //         Name = "An invalid Name",
    //         Description = "An invalid Description",
    //         Allergens = new List<dynamic>
    //         {
    //             new { Name = "An invalid Name" }
    //         },
    //         ProductTags = new List<dynamic>
    //         {
    //             new { Name = "An invalid Name" }
    //         }
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var responseBody = await ResponseUtils.DeserializeContentAsync(response);
    //
    //     responseBody.Should().MatchRegex(@"Name.*Must be lowercase\.");
    //     responseBody.Should().MatchRegex(@"Description.*Must be lowercase\.");
    //     responseBody.Should().MatchRegex(@"Allergens.*Must be lowercase\.");
    //     responseBody.Should().MatchRegex(@"ProductTags.*Must be lowercase\.");
    // }
    //
    //
    // [Fact]
    // public async Task CreateOrder_Should_Return_A_401Unauthorized()
    // {
    //     // Arrange
    //     var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
    //     var restaurant = await CreateRestaurant(decodedTokens.AccessToken, RestaurantMocks.SimpleRestaurantCreateDto);
    //     Client.SetAuthorizationHeader(TokenMocks.ExpiredAccessToken);
    //     var dateTime = DateTime.UtcNow;
    //     var productCreateDto = ProductMocks.ProductCreateDto;
    //     var body = new
    //     {
    //         productCreateDto.Name,
    //         productCreateDto.Price,
    //         productCreateDto.Quantity,
    //         productCreateDto.IsActive,
    //         productCreateDto.ProductType,
    //         ExpirationDateTime = dateTime.AddDays(1),
    //         productCreateDto.Description,
    //         productCreateDto.Allergens,
    //         productCreateDto.ProductTags
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);
    //
    //     // Assert
    //     await AssertResponseUtils.AssertUnauthorizedResponse(response);
    // }
    //
    // [Fact]
    // public async Task CreateOrder_Should_Return_A_403Forbidden_When_User_Is_Not_Owner_Of_The_Restaurant()
    // {
    //     // Arrange
    //     var dateTime = DateTime.UtcNow;
    //     var decodedTokensOfRestaurantAdmin = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
    //     var restaurant = await CreateRestaurant(decodedTokensOfRestaurantAdmin.AccessToken, RestaurantMocks.SimpleRestaurantCreateDto);
    //     var decodedTokensOfUser = await CreateAndLoginUser(UserMocks.CustomerCreateDto);
    //     Client.SetAuthorizationHeader(decodedTokensOfUser.AccessToken);
    //     var productCreateDto = ProductMocks.ProductCreateDto;
    //     var body = new
    //     {
    //         productCreateDto.Name,
    //         productCreateDto.Price,
    //         productCreateDto.Quantity,
    //         productCreateDto.IsActive,
    //         productCreateDto.ProductType,
    //         ExpirationDateTime = dateTime.AddDays(1),
    //         productCreateDto.Description,
    //         productCreateDto.Allergens,
    //         productCreateDto.ProductTags
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);
    //
    //     // Assert
    //     await AssertResponseUtils.AssertForbiddenResponse(response);
    // }

    #endregion
}
