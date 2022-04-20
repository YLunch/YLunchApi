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
            ProductIds = new[] { product1.Id, product2.Id, "badIdFormat" },
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
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.OrderStatuses.Count.Should().Be(1);
        responseBody.OrderStatuses.ElementAt(0).Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.OrderStatuses.ElementAt(0).OrderId.Should().Be(responseBody.Id);
        responseBody.OrderStatuses.ElementAt(0).DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.OrderStatuses.ElementAt(0).State.Should().Be(OrderState.Idling);
        responseBody.CustomerComment.Should().Be("Customer comment");
        responseBody.RestaurantComment.Should().BeNull();
        responseBody.IsAccepted.Should().Be(false);
        responseBody.IsAcknowledged.Should().Be(false);
        responseBody.IsDeleted.Should().Be(false);
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
                        ExpirationDateTime = x.ExpirationDateTime,
                        ProductType = x.ProductType,
                        Image = x.Image,
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

    #region GetOrdersTests

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_200Ok_With_Correct_Orders_When_User_Is_A_RestaurantAdmin()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto1 = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto1.Email = $"1{restaurantAdminCreateDto1.Email}";
        var restaurantAdmin1DecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto1);

        var restaurantAdminCreateDto2 = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto2.Email = $"2{restaurantAdminCreateDto2.Email}";
        var restaurantAdmin2DecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto2);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(restaurantAdmin1DecodedTokens.AccessToken, restaurantCreateDto1);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(restaurantAdmin2DecodedTokens.AccessToken, restaurantCreateDto2);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdmin1DecodedTokens.AccessToken, restaurant1.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdmin1DecodedTokens.AccessToken, restaurant1.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdmin1DecodedTokens.AccessToken, restaurant1.Id, productCreateDto3);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(restaurantAdmin2DecodedTokens.AccessToken, restaurant2.Id, productCreateDto4);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(restaurantAdmin2DecodedTokens.AccessToken, restaurant2.Id, productCreateDto5);

        var order1 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant1.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        var order2 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant2.Id, new List<ProductReadDto>
        {
            product4,
            product5
        });

        var order3 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant1.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader(restaurantAdmin1DecodedTokens.AccessToken);

        // Act
        var response = await Client.GetAsync($"restaurants/{restaurant1.Id}/orders?page=1&size=2&minCreationDateTime={DateTime.UtcNow}&maxCreationDateTime={DateTime.UtcNow}&orderStates=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<OrderReadDto>>(response);

        responseBody.Count.Should().Be(2);

        responseBody[0].Should().BeEquivalentTo(order1);
        responseBody[1].Should().BeEquivalentTo(order3);
        foreach (var order in responseBody)
        {
            order.Should().NotBeEquivalentTo(order2);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_401Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);

        await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader("");

        // Act
        var response = await Client.GetAsync($"restaurants/{restaurant.Id}/orders");

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_403Forbidden_When_User_Is_A_Customer()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);

        await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader(customerDecodedTokens.AccessToken);

        // Act
        var response = await Client.GetAsync($"restaurants/{restaurant.Id}/orders");

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_Should_Return_A_200Ok_With_Correct_Orders_When_User_Is_A_Customer()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto1 = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto1.Email = $"1{restaurantAdminCreateDto1.Email}";
        var restaurantAdmin1DecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto1);

        var restaurantAdminCreateDto2 = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto2.Email = $"2{restaurantAdminCreateDto2.Email}";
        var restaurantAdmin2DecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto2);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(restaurantAdmin1DecodedTokens.AccessToken, restaurantCreateDto1);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(restaurantAdmin2DecodedTokens.AccessToken, restaurantCreateDto2);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdmin1DecodedTokens.AccessToken, restaurant1.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdmin1DecodedTokens.AccessToken, restaurant1.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdmin1DecodedTokens.AccessToken, restaurant1.Id, productCreateDto3);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(restaurantAdmin2DecodedTokens.AccessToken, restaurant2.Id, productCreateDto4);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(restaurantAdmin2DecodedTokens.AccessToken, restaurant2.Id, productCreateDto5);

        var order1 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant1.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        var order2 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant2.Id, new List<ProductReadDto>
        {
            product4,
            product5
        });

        var order3 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant1.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader(customerDecodedTokens.AccessToken);

        // Act
        var response = await Client.GetAsync($"orders?page=1&size=2&restaurantId={restaurant1.Id}&minCreationDateTime={DateTime.UtcNow}&maxCreationDateTime={DateTime.UtcNow}&orderStates=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<OrderReadDto>>(response);

        responseBody.Count.Should().Be(2);

        responseBody[0].Should().BeEquivalentTo(order1);
        responseBody[1].Should().BeEquivalentTo(order3);
        foreach (var order in responseBody)
        {
            order.Should().NotBeEquivalentTo(order2);
        }
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_Should_Return_A_401Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);

        await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader("");

        // Act
        var response = await Client.GetAsync("orders");

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_Should_Return_A_403Forbidden_When_User_Is_A_RestaurantAdmin()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);

        await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader(restaurantAdminDecodedTokens.AccessToken);

        // Act
        var response = await Client.GetAsync("orders");

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    #endregion

    #region AddStatusToOrders

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_200Ok_With_Updated_Orders()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);

        var order1 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        var order3 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        var orders = new List<OrderReadDto> { order1, order3 };

        Client.SetAuthorizationHeader(restaurantAdminDecodedTokens.AccessToken);

        var body = new
        {
            OrderIds = new[] { order1.Id, order3.Id },
            OrderState = OrderState.Acknowledged
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders/statuses", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<OrderReadDto>>(response);

        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.State.Should().Be(OrderState.Acknowledged);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses
                                                                           .Concat(new List<OrderStatusReadDto>
                                                                           {
                                                                               responseBody[i].CurrentOrderStatus
                                                                           }));
        }
    }

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_400BadRequest_When_Empty_Fields()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        Client.SetAuthorizationHeader(restaurantAdminDecodedTokens.AccessToken);

        var body = new { };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders/statuses", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("The OrderIds field is required.");
        responseBody.Should().Contain("The OrderState field is required.");
    }

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_400BadRequest_When_Invalid_Fields()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        Client.SetAuthorizationHeader(restaurantAdminDecodedTokens.AccessToken);

        var body = new
        {
            OrderIds = Array.Empty<dynamic>(),
            OrderState = 10
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders/statuses", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().MatchRegex(@"OrderIds.*Must be a list of id which match Guid regular expression\.");
        responseBody.Should().Contain($"The field OrderState must be between 0 and {OrderStateUtils.Count - 1}.");
    }

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_401Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);

        var order1 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        var order3 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader("");

        var body = new
        {
            OrderIds = new[] { order1.Id, order3.Id },
            OrderState = OrderState.Acknowledged
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders/statuses", body);

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_403Forbidden_When_User_Is_A_Customer()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        var restaurantAdminCreateDto = UserMocks.RestaurantAdminCreateDto;
        restaurantAdminCreateDto.Email = $"{restaurantAdminCreateDto.Email}";
        var restaurantAdminDecodedTokens = await CreateAndLoginUser(restaurantAdminCreateDto);

        var customerDecodedTokens = await CreateAndLoginUser(UserMocks.CustomerCreateDto);

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(restaurantAdminDecodedTokens.AccessToken, restaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(restaurantAdminDecodedTokens.AccessToken, restaurant.Id, productCreateDto3);


        var order1 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        var order3 = await CreateOrder(customerDecodedTokens.AccessToken, restaurant.Id, new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        });

        Client.SetAuthorizationHeader(customerDecodedTokens.AccessToken);

        var body = new
        {
            OrderIds = new[] { order1.Id, order3.Id },
            OrderState = OrderState.Acknowledged
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/orders/statuses", body);

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    #endregion
}
