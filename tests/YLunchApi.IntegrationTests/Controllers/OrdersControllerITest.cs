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
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;
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

    [Fact]
    public async Task CreateOrder_Should_Return_A_400BadRequest_When_Missing_Fields()
    {
        // Arrange
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, RestaurantMocks.PrepareFullRestaurant("restaurant", DateTime.UtcNow));

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(customerDecodedTokens.AccessToken);

        var body = new { };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("The ProductIds field is required.");
        responseBody.Should().Contain("The ReservedForDateTime field is required.");
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_400BadRequest_When_Invalid_Fields()
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

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(customerDecodedTokens.AccessToken);

        var body = new
        {
            ProductIds = new List<dynamic> { product1.Id, product2.Id, "badIdFormat" },
            ReservedForDateTime = DateTime.UtcNow.AddHours(-2)
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().MatchRegex(@"ProductIds.*Must be a list of id which match Guid regular expression\.");
        responseBody.Should().MatchRegex(@"ReservedForDateTime.*DateTime must be in future if present\.");
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_401Unauthorized()
    {
        // Arrange
        var decodedTokens = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        var restaurant = await CreateRestaurant(decodedTokens.AccessToken, RestaurantMocks.SimpleRestaurantCreateDto);
        Client.SetAuthorizationHeader(TokenMocks.ExpiredAccessToken);
        var body = new { };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders", body);

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_403Forbidden_When_User_Is_Not_Customer()
    {
        // Arrange
        var decodedTokensOfRestaurantAdmin = await CreateAndLoginUser(UserMocks.RestaurantAdminCreateDto);
        var restaurant = await CreateRestaurant(decodedTokensOfRestaurantAdmin.AccessToken, RestaurantMocks.SimpleRestaurantCreateDto);
        Client.SetAuthorizationHeader(decodedTokensOfRestaurantAdmin.AccessToken);
        var body = new { };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders", body);

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    #endregion

    #region GetOrderByIdTests

    [Fact]
    public async Task GetOrderById_Should_Return_A_200Ok()
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

        var order = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto> { product1, product2, product3 });

        // Act
        var response = await Client.GetAsync($"orders/{order.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<OrderReadDto>(response);

        var dateTime = DateTime.UtcNow;
        var products = new List<ProductReadDto> { product1, product2, product3 };
        var customerId = customerDecodedTokens.UserId;

        responseBody.Id.Should().Be(order.Id);
        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.UserId.Should().Be(customerId);
        responseBody.OrderStatuses.Count.Should().Be(1);
        responseBody.OrderStatuses.ElementAt(0).Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.OrderStatuses.ElementAt(0).OrderId.Should().Be(responseBody.Id);
        responseBody.OrderStatuses.ElementAt(0).DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.OrderStatuses.ElementAt(0).State.Should().Be(OrderState.Idling);
        responseBody.CustomerComment.Should().Be("Customer comment");
        responseBody.RestaurantComment.Should().BeNull();
        responseBody.IsAccepted.Should().Be(false);
        responseBody.IsAcknowledged.Should().Be(false);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ReservedForDateTime.Should().BeCloseTo(dateTime.AddHours(1), TimeSpan.FromSeconds(5));
        responseBody.OrderedProducts.Count.Should().Be(products.Count);
        responseBody.OrderedProducts
                    .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderedProducts.Should().BeEquivalentTo(
            products
                .Select(x =>
                {
                    var orderedProductReadDto = new OrderedProductReadDto
                    {
                        OrderId = responseBody.Id,
                        ProductId = x.Id,
                        UserId = customerId,
                        RestaurantId = x.RestaurantId,
                        Name = x.Name,
                        Description = x.Description,
                        Price = x.Price,
                        CreationDateTime = x.CreationDateTime,
                        Allergens = string.Join(",", x.Allergens.Select(y => y.Name).OrderBy(y => y)),
                        ProductTags = string.Join(",", x.ProductTags.Select(y => y.Name).OrderBy(y => y))
                    };
                    return orderedProductReadDto;
                })
                .ToList(), options => options.Excluding(x => x.Id));
        responseBody.TotalPrice.Should().Be(responseBody.OrderedProducts.Sum(x => x.Price));
        responseBody.CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.CurrentOrderStatus.OrderId.Should().Be(responseBody.Id);
        responseBody.CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.CurrentOrderStatus.State.Should().Be(OrderState.Idling);
    }

    #endregion
}
