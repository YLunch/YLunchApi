using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.CommonAggregate.Services;
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

    #region Utils

    private OrdersController InitOrdersController(string accessToken, DateTime dateTime)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(dateTime);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = accessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        return Fixture.GetImplementationFromService<OrdersController>();
    }

    #endregion

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
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var customerId = new ApplicationSecurityToken(TokenMocks.ValidCustomerAccessToken).UserId;

        var ordersController = InitOrdersController(TokenMocks.ValidCustomerAccessToken, dateTime);

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
        responseBody.OrderStatuses.ElementAt(0).State.Should().Be(OrderState.Idling);
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
        responseBody.CurrentOrderStatus.State.Should().Be(OrderState.Idling);
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

        var ordersController = InitOrdersController(TokenMocks.ValidCustomerAccessToken, dateTime);

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

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.BadRequest, "ReservedForDateTime must be set when the restaurant is open for orders."));
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

        var ordersController = InitOrdersController(TokenMocks.ValidCustomerAccessToken, dateTime);

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

        var ordersController = InitOrdersController(TokenMocks.ValidCustomerAccessToken, dateTime);

        var notExistingProductId = Guid.NewGuid().ToString();
        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                notExistingProductId,
                product2.Id
            },
            ReservedForDateTime = dateTime.AddHours(1),
            CustomerComment = "Customer comment"
        };


        // Act
        var response = await ordersController.CreateOrder(restaurant.Id, orderCreateDto);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, $"Product: {notExistingProductId} not found."));
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

        var ordersController = InitOrdersController(TokenMocks.ValidCustomerAccessToken, dateTime);

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
        responseBody.OrderStatuses.ElementAt(0).State.Should().Be(OrderState.Idling);
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
        responseBody.CurrentOrderStatus.State.Should().Be(OrderState.Idling);
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

        var ordersController = InitOrdersController(TokenMocks.ValidRestaurantAdminAccessToken, dateTime);

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
        responseBody.OrderStatuses.ElementAt(0).State.Should().Be(OrderState.Idling);
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
        responseBody.CurrentOrderStatus.State.Should().Be(OrderState.Idling);
    }

    [Fact]
    public async Task GetOrderById_Should_Return_A_404NotFound_When_OrderId_Does_Not_Exist()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var ordersController = InitOrdersController(TokenMocks.ValidCustomerAccessToken, dateTime);
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

        var ordersController = InitOrdersController(TokenMocks.ValidCustomer2AccessToken, dateTime);

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

        var ordersController = InitOrdersController(TokenMocks.ValidRestaurantAdmin2AccessToken, dateTime);

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
    public async Task GetOrdersByRestaurantId_Should_Return_A_200Ok_With_Correct_Orders()
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

        var ordersController = InitOrdersController(TokenMocks.ValidRestaurantAdminAccessToken, dateTime);

        // Act
        var response = await ordersController.GetOrdersByRestaurantId(restaurant1.Id, new OrderFilter
        {
            OrderStates = new SortedSet<OrderState>{OrderState.Idling}
        });

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<OrderReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        responseBody[0].Should().BeEquivalentTo(order1);
        responseBody[1].Should().BeEquivalentTo(order3);
        foreach (var order in responseBody)
        {
            order.Should().NotBeEquivalentTo(order2);
        }
    }

    [Fact]
    public async Task GetOrdersByRestaurantId_Should_Return_A_404NotFound_When_Restaurant_Is_Not_Found()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;

        var ordersController = InitOrdersController(TokenMocks.ValidRestaurantAdminAccessToken, dateTime);

        var restaurantId = Guid.NewGuid().ToString();

        // Act
        var response = await ordersController.GetOrdersByRestaurantId(restaurantId);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Restaurant: {restaurantId} not found."));
    }

    #endregion
}
