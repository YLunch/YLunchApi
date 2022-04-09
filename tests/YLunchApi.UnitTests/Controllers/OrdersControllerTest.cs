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

    private OrdersController InitOrdersController(string accessToken, DateTime? customDateTime = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
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
        var restaurant = await CreateRestaurant(restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurant.Id, dateTime, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurant.Id, dateTime, productCreateDto2);

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
        responseBody.OrderStatuses.Count.Should().Be(1);
        responseBody.UserId.Should().Be(customerId);
        responseBody.OrderStatuses.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderStatuses.Should().BeEquivalentTo(new List<OrderStatusReadDto>
        {
            new()
            {
                OrderId = responseBody.Id,
                DateTime = dateTime,
                State = OrderState.Idling
            }
        }, options => options.Excluding(x => x.Id));
        responseBody.CustomerComment.Should().Be("Customer comment");
        responseBody.RestaurantComment.Should().BeNull();
        responseBody.IsAccepted.Should().Be(false);
        responseBody.IsAcknowledged.Should().Be(false);
        responseBody.CreationDateTime.Should().Be(dateTime);
        responseBody.ReservedForDateTime.Should().BeCloseTo(dateTime.AddHours(1), TimeSpan.FromSeconds(5));
        responseBody.OrderedProducts.Count.Should().Be(2);
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
                        Allergens = string.Join(",", x.Allergens.Select(y => y.Name).OrderBy(y => y)),
                        ProductTags = string.Join(",", x.ProductTags.Select(y => y.Name).OrderBy(y => y))
                    };
                    return orderedProductReadDto;
                })
                .ToList(), options => options.Excluding(x => x.Id));
        responseBody.TotalPrice.Should().Be(responseBody.OrderedProducts.Sum(x => x.Price));
        responseBody.CurrentOrderStatus.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.CurrentOrderStatus.Should().BeEquivalentTo(new OrderStatusReadDto
        {
            OrderId = responseBody.Id,
            DateTime = dateTime,
            State = OrderState.Idling
        }, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task CreateOrder_Should_Return_A_400BadRequest_When_ReservedForDateTime_Is_Out_Of_OrderOpeningTimes()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.PrepareFullRestaurant(RestaurantMocks.SimpleRestaurantCreateDto.Name, dateTime);
        var restaurant = await CreateRestaurant(restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurant.Id, dateTime, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurant.Id, dateTime, productCreateDto2);

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
        var restaurant = await CreateRestaurant(restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        var product1 = await CreateProduct(restaurant.Id, dateTime, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurant.Id, dateTime, productCreateDto2);

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
        var restaurant = await CreateRestaurant(restaurantCreateDto, dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        await CreateProduct(restaurant.Id, dateTime, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(restaurant.Id, dateTime, productCreateDto2);

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
}