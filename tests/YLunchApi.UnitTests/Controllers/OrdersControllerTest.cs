using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Controllers;

public class OrdersControllerTest : UnitTestFixture
{
    public OrdersControllerTest(UnitTestFixtureBase fixture) : base(fixture)
    {
    }

    #region CreateOrderTests

    [Fact]
    public async Task CreateOrder_Should_Return_A_201Created()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.Quantity = 10;
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        productCreateDto2.Quantity = null;
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var customerId = new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken).UserId;

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };

        // Act
        var response = await ordersController.CreateOrder(restaurant.Id, orderCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<OrderReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.UserId.Should().Be(customerId);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.OrderStatuses.Count.Should().Be(1);
        responseBody.OrderStatuses.ElementAt(0).Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.OrderStatuses.ElementAt(0).OrderId.Should().Be(responseBody.Id);
        responseBody.OrderStatuses.ElementAt(0).DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.OrderStatuses.ElementAt(0).OrderState.Should().Be(OrderState.Idling);
        responseBody.CustomerComment.Should().Be("Customer comment");
        responseBody.RestaurantComment.Should().BeNull();
        responseBody.IsAccepted.Should().Be(false);
        responseBody.IsAcknowledged.Should().Be(false);
        responseBody.IsDeleted.Should().Be(false);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ReservedForDateTime.Should().BeCloseTo(dateTime.AddHours(1), TimeSpan.FromSeconds(5));
        responseBody.OrderedProducts.Count.Should().Be(orderCreateDto.ProductIds.Count);
        responseBody.OrderedProducts
                    .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderedProducts.Should().BeEquivalentTo(
            new List<ProductReadDto> { product1, product1, product2 }
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
        responseBody.CurrentOrderStatus.OrderState.Should().Be(OrderState.Idling);
        var productsController = InitController<ProductsController>(new FixtureConfiguration
        {
            DateTime = dateTime
        });

        var getProductByIdResponse = await productsController.GetProductById(product1.Id);
        var getProductByIdResponseResult = Assert.IsType<OkObjectResult>(getProductByIdResponse.Result);
        var updatedProduct1 = Assert.IsType<ProductReadDto>(getProductByIdResponseResult.Value);
        updatedProduct1.Quantity.Should().Be(8);
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_400BadRequest_When_Products_Are_Sold_Out()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.Quantity = 1;
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        productCreateDto2.Quantity = null;
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var customerId = new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken).UserId;

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };

        // Act
        var response = await ordersController.CreateOrder(restaurant.Id, orderCreateDto);

        // Assert
        var responseResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.BadRequest, $"Product(s): {product1.Id} sold out."));
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_400BadRequest_When_ReservedForDateTime_Is_Out_Of_OrderOpeningTimes()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(3),
            CustomerComment = "Customer comment"
        };

        // Act
        var response = await ordersController.CreateOrder(restaurant.Id, orderCreateDto);

        // Assert
        var responseResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.BadRequest, "ReservedForDateTime must be set when the restaurant is open in place."));
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_404NotFound_When_Restaurant_Is_Not_Found()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(3),
            CustomerComment = "Customer comment"
        };

        var notExistingRestaurantId = Guid.NewGuid().ToString();

        // Act
        var response = await ordersController.CreateOrder(notExistingRestaurantId, orderCreateDto);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {notExistingRestaurantId} not found."));
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_404NotFound_When_A_Product_Is_Not_Found()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        var notExistingProductId1 = Guid.NewGuid().ToString();
        var notExistingProductId2 = Guid.NewGuid().ToString();
        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                notExistingProductId1,
                product2.Id,
                notExistingProductId2
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };


        // Act
        var response = await ordersController.CreateOrder(restaurant.Id, orderCreateDto);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, $"Products: {notExistingProductId1} and {notExistingProductId2} not found."));
    }

    #endregion

    #region GetOrderByIdTests

    [Fact]
    public async Task GetOrderById_Should_Return_A_200Ok_For_Customer()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var customerId = new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken).UserId;


        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };
        var order = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, orderCreateDto);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        // Act
        var response = await ordersController.GetOrderById(order.Id);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<OrderReadDto>(responseResult.Value);

        responseBody.Id.Should().Be(order.Id);
        responseBody.UserId.Should().Be(customerId);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.OrderStatuses.Count.Should().Be(1);
        responseBody.OrderStatuses.ElementAt(0).Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.OrderStatuses.ElementAt(0).OrderId.Should().Be(responseBody.Id);
        responseBody.OrderStatuses.ElementAt(0).DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.OrderStatuses.ElementAt(0).OrderState.Should().Be(OrderState.Idling);
        responseBody.CustomerComment.Should().Be("Customer comment");
        responseBody.RestaurantComment.Should().BeNull();
        responseBody.IsAccepted.Should().Be(false);
        responseBody.IsAcknowledged.Should().Be(false);
        responseBody.IsDeleted.Should().Be(false);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ReservedForDateTime.Should().BeCloseTo(dateTime.AddHours(1), TimeSpan.FromSeconds(5));
        responseBody.OrderedProducts.Count.Should().Be(orderCreateDto.ProductIds.Count);
        responseBody.OrderedProducts
                    .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderedProducts.Should().BeEquivalentTo(
            new List<ProductReadDto> { product1, product2 }
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
        responseBody.CurrentOrderStatus.OrderState.Should().Be(OrderState.Idling);
    }

    [Fact]
    public async Task GetOrderById_Should_Return_A_200Ok_For_RestaurantAdmin()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var customerId = new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken).UserId;


        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };
        var order = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, orderCreateDto);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        // Act
        var response = await ordersController.GetOrderById(order.Id);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<OrderReadDto>(responseResult.Value);


        responseBody.Id.Should().Be(order.Id);
        responseBody.UserId.Should().Be(customerId);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.OrderStatuses.Count.Should().Be(1);
        responseBody.OrderStatuses.ElementAt(0).Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.OrderStatuses.ElementAt(0).OrderId.Should().Be(responseBody.Id);
        responseBody.OrderStatuses.ElementAt(0).DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.OrderStatuses.ElementAt(0).OrderState.Should().Be(OrderState.Idling);
        responseBody.CustomerComment.Should().Be("Customer comment");
        responseBody.RestaurantComment.Should().BeNull();
        responseBody.IsAccepted.Should().Be(false);
        responseBody.IsAcknowledged.Should().Be(false);
        responseBody.IsDeleted.Should().Be(false);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ReservedForDateTime.Should().BeCloseTo(dateTime.AddHours(1), TimeSpan.FromSeconds(5));
        responseBody.OrderedProducts.Count.Should().Be(orderCreateDto.ProductIds.Count);
        responseBody.OrderedProducts
                    .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderedProducts.Should().BeEquivalentTo(
            new List<ProductReadDto> { product1, product2 }
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
        responseBody.CurrentOrderStatus.OrderState.Should().Be(OrderState.Idling);
    }

    [Fact]
    public async Task GetOrderById_Should_Return_A_404NotFound_When_OrderId_Does_Not_Exist()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });
        var notExistingOrderId = Guid.NewGuid().ToString();

        // Act
        var response = await ordersController.GetOrderById(notExistingOrderId);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Order: {notExistingOrderId} not found."));
    }

    [Fact]
    public async Task GetOrderById_Should_Return_A_404NotFound_For_Customer_When_Order_Is_Not_Owned_By_Customer()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };
        var order = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, orderCreateDto);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidCustomer2AccessToken, DateTime = dateTime });

        // Act
        var response = await ordersController.GetOrderById(order.Id);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Order: {order.Id} not found."));
    }

    [Fact]
    public async Task GetOrderById_Should_Return_A_404NotFound_For_RestaurantAdmin_When_Order_Is_Not_Related_To_One_Of_His_Restaurants()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };
        var order = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, orderCreateDto);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdmin2AccessToken, DateTime = dateTime });

        // Act
        var response = await ordersController.GetOrderById(order.Id);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Order: {order.Id} not found."));
    }

    #endregion

    #region GetOrdersTests

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_200Ok_With_Correct_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order4 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var addStatusToOrdersResponse = await ordersController.AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string>
            {
                order1.Id,
                order3.Id
            },
            OrderState = OrderState.Acknowledged
        });

        var addStatusToOrdersResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrdersResponse.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrdersResponseResult.Value);
        orders.Add(order4);

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurant1.Id);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_With_Pagination_Should_Return_A_200Ok_With_Correct_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order2 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order4 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime.AddDays(-1), new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order5 = await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment6",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment7",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment8",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime })
            .AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order1.Id,
                    order3.Id
                },
                OrderState = OrderState.Acknowledged
            });

        await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdmin2AccessToken, DateTime = dateTime.AddSeconds(1) })
            .AddStatusToOrders(restaurant2.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order2.Id
                },
                OrderState = OrderState.Acknowledged
            });

        var orders = new List<OrderReadDto> { order4, order5 }
                     .OrderBy(x => x.CreationDateTime)
                     .ThenBy(x => x.CurrentOrderStatus.DateTime)
                     .ToList();

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurant1.Id, new OrderFilter
        {
            Size = 2,
            Page = 2
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(orders[i].OrderStatuses.Last().DateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_200Ok_With_Correct_Filtered_By_State_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order5 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime.AddDays(-1), new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var addStatusToOrdersResponse = await ordersController.AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string>
            {
                order1.Id,
                order3.Id,
                order5.Id
            },
            OrderState = OrderState.Acknowledged
        });

        var addStatusToOrdersResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrdersResponse.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrdersResponseResult.Value);

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurant1.Id, new OrderFilter
        {
            OrderStates = new SortedSet<OrderState> { OrderState.Acknowledged }
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(OrderState.Acknowledged);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_200Ok_With_Correct_Filtered_By_MinCreationDateTime_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order5 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime.AddDays(-1), new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var addStatusToOrdersResponse = await ordersController.AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string>
            {
                order1.Id,
                order3.Id,
                order5.Id
            },
            OrderState = OrderState.Acknowledged
        });

        var addStatusToOrdersResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrdersResponse.Result);
        var addStatusToOrdersResponseBody = Assert.IsType<List<OrderReadDto>>(addStatusToOrdersResponseResult.Value);

        var orders = new List<OrderReadDto>
        {
            addStatusToOrdersResponseBody.First(x => x.Id == order1.Id),
            addStatusToOrdersResponseBody.First(x => x.Id == order3.Id),
        };

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurant1.Id, new OrderFilter
        {
            OrderStates = new SortedSet<OrderState> { OrderState.Acknowledged },
            MinCreationDateTime = dateTime
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_200Ok_With_Correct_Filtered_By_MaxCreationDateTime_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order5 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime.AddDays(-1), new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var addStatusToOrdersResponse = await ordersController.AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string>
            {
                order1.Id,
                order3.Id,
                order5.Id
            },
            OrderState = OrderState.Acknowledged
        });

        var addStatusToOrdersResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrdersResponse.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrdersResponseResult.Value);

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurant1.Id, new OrderFilter
        {
            OrderStates = new SortedSet<OrderState> { OrderState.Acknowledged },
            MaxCreationDateTime = dateTime.AddDays(-1)
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(1);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_200Ok_With_Correct_Filtered_By_MinCreationDateTime_And_MaxCreationDateTime_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order5 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime.AddDays(-1), new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var addStatusToOrdersResponse = await ordersController.AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string>
            {
                order1.Id,
                order3.Id,
                order5.Id
            },
            OrderState = OrderState.Acknowledged
        });

        var addStatusToOrdersResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrdersResponse.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrdersResponseResult.Value);

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurant1.Id, new OrderFilter
        {
            OrderStates = new SortedSet<OrderState> { OrderState.Acknowledged },
            MinCreationDateTime = dateTime.AddDays(-1),
            MaxCreationDateTime = dateTime.AddDays(-1)
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(1);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_200Ok_With_Correct_Filtered_By_Customer_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order5 = await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment6",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order7 = await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment7",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment8",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var addStatusToOrdersResponse = await ordersController.AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string>
            {
                order5.Id,
                order7.Id
            },
            OrderState = OrderState.Acknowledged
        });

        var addStatusToOrdersResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrdersResponse.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrdersResponseResult.Value);

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurant1.Id, new OrderFilter
        {
            OrderStates = new SortedSet<OrderState> { OrderState.Acknowledged },
            MinCreationDateTime = dateTime,
            MaxCreationDateTime = dateTime,
            CustomerId = new ApplicationSecurityToken(TokenMocks.ValidCustomer2AccessToken).UserId
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfRestaurant_Should_Return_A_404NotFound_When_Restaurant_Is_Not_Found()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var restaurantId = Guid.NewGuid().ToString();

        // Act
        var response = await ordersController.GetOrdersOfRestaurant(restaurantId);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Restaurant: {restaurantId} not found."));
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_Should_Return_A_200Ok_With_Correct_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order2 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order4 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime.AddDays(-1), new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment6",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment7",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment8",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var addStatusToOrders1Response = await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime })
            .AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order1.Id,
                    order3.Id
                },
                OrderState = OrderState.Acknowledged
            });
        var addStatusToOrders1ResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrders1Response.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrders1ResponseResult.Value);

        var addStatusToOrders2Response = await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdmin2AccessToken, DateTime = dateTime.AddSeconds(1) })
            .AddStatusToOrders(restaurant2.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order2.Id
                },
                OrderState = OrderState.Acknowledged
            });
        var addStatusToOrders2ResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrders2Response.Result);
        order2 = Assert.IsType<List<OrderReadDto>>(addStatusToOrders2ResponseResult.Value)[0];

        orders.Add(order2);
        orders.Add(order4);
        orders = orders
                 .OrderBy(x => x.CreationDateTime)
                 .ThenBy(x => x.CurrentOrderStatus.DateTime)
                 .ToList();

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        // Act
        var response = await ordersController.GetOrdersOfCurrentCustomer();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(4);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(orders[i].OrderStatuses.Last().DateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_With_Pagination_Should_Return_A_200Ok_With_Correct_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order2 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order4 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime.AddDays(-1), new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment9",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment6",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment7",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment8",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var addStatusToOrders1Response = await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime })
            .AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order1.Id,
                    order3.Id
                },
                OrderState = OrderState.Acknowledged
            });
        var addStatusToOrders1ResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrders1Response.Result);
        order3 = Assert.IsType<List<OrderReadDto>>(addStatusToOrders1ResponseResult.Value)[1];

        await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdmin2AccessToken, DateTime = dateTime.AddSeconds(1) })
            .AddStatusToOrders(restaurant2.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order2.Id
                },
                OrderState = OrderState.Acknowledged
            });

        var orders = new List<OrderReadDto> { order3, order4 }
                     .OrderBy(x => x.CreationDateTime)
                     .ThenBy(x => x.CurrentOrderStatus.DateTime)
                     .ToList();

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        // Act
        var response = await ordersController.GetOrdersOfCurrentCustomer(new OrderFilter
        {
            Size = 2,
            Page = 2
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(orders[i].OrderStatuses.Last().DateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_Should_Return_A_200Ok_With_Correct_Filtered_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order2 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment6",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment7",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment8",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var addStatusToOrders1Response = await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime })
            .AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order1.Id,
                    order3.Id
                },
                OrderState = OrderState.Acknowledged
            });
        var addStatusToOrders1ResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrders1Response.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrders1ResponseResult.Value);

        var addStatusToOrders2Response = await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdmin2AccessToken, DateTime = dateTime.AddSeconds(1) })
            .AddStatusToOrders(restaurant2.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order2.Id
                },
                OrderState = OrderState.Acknowledged
            });
        var addStatusToOrders2ResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrders2Response.Result);
        order2 = Assert.IsType<List<OrderReadDto>>(addStatusToOrders2ResponseResult.Value)[0];

        orders.Add(order2);
        orders = orders
                 .OrderBy(x => x.CreationDateTime)
                 .ThenBy(x => x.CurrentOrderStatus.DateTime)
                 .ToList();

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        // Act
        var response = await ordersController.GetOrdersOfCurrentCustomer(new OrderFilter
        {
            OrderStates = new SortedSet<OrderState> { OrderState.Acknowledged },
            MinCreationDateTime = dateTime,
            MaxCreationDateTime = dateTime,
            // Random id. Useless since it's set in the endpoint with the current user id
            CustomerId = Guid.NewGuid().ToString()
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_Should_Return_A_200Ok_With_Correct_Filtered_By_RestaurantId_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order2 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddDays(-1).AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment5",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment6",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment7",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomer2AccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment8",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdmin2AccessToken, DateTime = dateTime })
            .AddStatusToOrders(restaurant2.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order2.Id
                },
                OrderState = OrderState.Acknowledged
            });

        var addStatusToOrdersResponse = await InitController<OrdersController>(new FixtureConfiguration
            {
                AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
            })
            .AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
            {
                OrderIds = new SortedSet<string>
                {
                    order1.Id,
                    order3.Id
                },
                OrderState = OrderState.Acknowledged
            });

        var addStatusToOrdersResponseResult = Assert.IsType<OkObjectResult>(addStatusToOrdersResponse.Result);
        var orders = Assert.IsType<List<OrderReadDto>>(addStatusToOrdersResponseResult.Value);

        var ordersController = InitController<OrdersController>(new FixtureConfiguration
        {
            AccessToken = TokenMocks.ValidCustomerAccessToken, DateTime = dateTime
        });

        // Act
        var response = await ordersController.GetOrdersOfCurrentCustomer(new OrderFilter
        {
            OrderStates = new SortedSet<OrderState> { OrderState.Acknowledged },
            MinCreationDateTime = dateTime,
            MaxCreationDateTime = dateTime,
            RestaurantId = restaurant1.Id,
            // Random id. Useless since it's set in the endpoint with the current user id
            CustomerId = Guid.NewGuid().ToString()
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(orders[i].OrderStatuses.Last().OrderState);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses);
        }
    }

    [Fact]
    public async Task GetOrdersOfCurrentCustomer_Should_Return_A_404NotFound_When_Restaurant_Is_Not_Found()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var restaurantId = Guid.NewGuid().ToString();

        // Act
        var response = await ordersController.GetOrdersOfCurrentCustomer(new OrderFilter
        {
            RestaurantId = restaurantId
        });

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Restaurant: {restaurantId} not found."));
    }

    #endregion

    #region AddStatusToOrdersTests

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_200Ok_With_Updated_Orders()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto3, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var orders = new List<OrderReadDto> { order1, order3 };

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        // Act
        var response = await ordersController.AddStatusToOrders(restaurant.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string> { order1.Id, order3.Id },
            OrderState = OrderState.Acknowledged
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            responseBody[i].CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
            responseBody[i].CurrentOrderStatus.OrderId.Should().Be(orders[i].Id);
            responseBody[i].CurrentOrderStatus.OrderState.Should().Be(OrderState.Acknowledged);
            responseBody[i].CurrentOrderStatus.DateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));

            responseBody[i].OrderStatuses.Should().BeEquivalentTo(orders[i].OrderStatuses
                                                                           .Concat(new List<OrderStatusReadDto>
                                                                           {
                                                                               responseBody[i].CurrentOrderStatus
                                                                           }));
        }
    }

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_404NotFound_When_Restaurant_Is_Not_Found()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto3, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var notExistingRestaurantId = Guid.NewGuid().ToString();

        // Act
        var response = await ordersController.AddStatusToOrders(notExistingRestaurantId, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string> { order1.Id, order3.Id },
            OrderState = OrderState.Acknowledged
        });

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {notExistingRestaurantId} not found."));
    }

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_404NotFound_When_An_Order_Is_Not_Found()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant("restaurant", dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto3, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        var notExistingOrderId1 = Guid.NewGuid().ToString();
        var notExistingOrderId2 = Guid.NewGuid().ToString();

        // Act
        var response = await ordersController.AddStatusToOrders(restaurant.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string> { order1.Id, notExistingOrderId1, order3.Id, notExistingOrderId2 },
            OrderState = OrderState.Acknowledged
        });

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, $"Orders: {string.Join(" and ", new SortedSet<string> { notExistingOrderId1, notExistingOrderId2 })} not found."));
    }

    [Fact]
    public async Task AddStatusToOrders_Should_Return_A_404NotFound_When_An_Order_Is_Not_For_The_Restaurant()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var restaurantCreateDto1 = RestaurantMocks.PrepareFullRestaurant("restaurant1", dateTime);
        var restaurant1 = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto1, dateTime);

        var restaurantCreateDto2 = RestaurantMocks.PrepareFullRestaurant("restaurant2", dateTime);
        var restaurant2 = await CreateRestaurant(TokenMocks.ValidRestaurantAdmin2AccessToken, restaurantCreateDto2, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant1.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant2.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant2.Id, productCreateDto5, dateTime);

        var order1 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment1",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order2 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment2",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var order3 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant1.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment3",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id,
                product3.Id
            }
        });

        var order4 = await CreateOrder(TokenMocks.ValidCustomerAccessToken, restaurant2.Id, dateTime, new OrderCreateDto
        {
            CustomerComment = "Customer comment4",
            ReservedForDateTime = dateTime.AddHours(1),
            ProductIds = new List<string>
            {
                product4.Id,
                product5.Id,
            }
        });

        var ordersController = InitController<OrdersController>(new FixtureConfiguration { AccessToken = TokenMocks.ValidRestaurantAdminAccessToken, DateTime = dateTime });

        // Act
        var response = await ordersController.AddStatusToOrders(restaurant1.Id, new AddStatusToOrdersDto
        {
            OrderIds = new SortedSet<string> { order1.Id, order2.Id, order3.Id, order4.Id },
            OrderState = OrderState.Acknowledged
        });

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, $"Orders: {string.Join(" and ", new SortedSet<string> { order2.Id, order4.Id })} not found."));
    }

    #endregion
}
