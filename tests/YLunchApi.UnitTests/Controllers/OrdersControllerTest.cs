using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Dto;
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

    private async Task<UserReadDto> CreateCustomer(DateTime? customDateTime = null, string? userEmail = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
        Fixture.InitFixture(configuration => { configuration.DateTimeProvider = dateTimeProviderMock.Object; });
        var customerCreateDto = UserMocks.CustomerCreateDto;
        customerCreateDto.Email = userEmail ?? customerCreateDto.Email;
        var usersController = Fixture.GetImplementationFromService<UsersController>();
        var response = await usersController.Create(customerCreateDto);
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        return Assert.IsType<UserReadDto>(responseResult.Value);
    }

    private async Task<RestaurantReadDto> CreateRestaurant(DateTime? customDateTime = null, string? restaurantName = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        var restaurantsController = Fixture.GetImplementationFromService<RestaurantsController>();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.Name = restaurantName ?? restaurantCreateDto.Name;
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        return Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
    }

    private async Task<ProductReadDto> CreateProduct(string restaurantId, DateTime? customDateTime = null, string? productName = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        var productsController = Fixture.GetImplementationFromService<ProductsController>();
        var productCreateDto = ProductMocks.ProductCreateDto;
        productCreateDto.Name = productName ?? productCreateDto.Name;
        var response = await productsController.CreateProduct(restaurantId, productCreateDto);
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        return Assert.IsType<ProductReadDto>(responseResult.Value);
    }

    #endregion

    #region CreateOrderTests

    [Fact]
    public async Task CreateOrder_Should_Return_A_201Created()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var customer = await CreateCustomer(dateTime);
        var restaurant = await CreateRestaurant(dateTime);
        var product1 = await CreateProduct(restaurant.Id, dateTime, "product1");
        var product2 = await CreateProduct(restaurant.Id, dateTime, "product2");
        var ordersController = Fixture.GetImplementationFromService<OrdersController>();
        var orderCreateDto = new OrderCreateDto
        {
            ProductIds = new List<string>
            {
                product1.Id,
                product2.Id
            },
            ReservedForDateTime = dateTime,
            CustomerComment = "customer comment"
        };

        // Act
        var response = await ordersController.CreateOrder(restaurant.Id, orderCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<OrderReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
    }

    #endregion
}
