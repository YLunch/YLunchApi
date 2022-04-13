using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Main.Controllers;

namespace YLunchApi.UnitTests.Core.Configuration;

public class UnitTestFixture : IClassFixture<UnitTestFixtureBase>
{
    protected readonly UnitTestFixtureBase Fixture;

    protected UnitTestFixture(UnitTestFixtureBase fixture)
    {
        Fixture = fixture;
        fixture.DatabaseId = Guid.NewGuid().ToString();
    }

    protected async Task<RestaurantReadDto> CreateRestaurant(string accessToken, RestaurantCreateDto restaurantCreateDto, DateTime customDateTime)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = accessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        var restaurantsController = Fixture.GetImplementationFromService<RestaurantsController>();
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        return Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
    }

    protected async Task<ProductReadDto> CreateProduct(string accessToken, string restaurantId, ProductCreateDto productCreateDto, DateTime dateTime)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(dateTime);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = accessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        var productsController = Fixture.GetImplementationFromService<ProductsController>();
        var response = await productsController.CreateProduct(restaurantId, productCreateDto);
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        return Assert.IsType<ProductReadDto>(responseResult.Value);
    }

    protected async Task<OrderReadDto> CreateOrder(string accessToken, string restaurantId, DateTime dateTime, OrderCreateDto orderCreateDto)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(dateTime);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = accessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        var ordersController = Fixture.GetImplementationFromService<OrdersController>();
        var response = await ordersController.CreateOrder(restaurantId, orderCreateDto);
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        return Assert.IsType<OrderReadDto>(responseResult.Value);
    }
}
