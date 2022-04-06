using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.UnitTests.Core.Configuration;

public class UnitTestFixture : IClassFixture<UnitTestFixtureBase>
{
    protected readonly UnitTestFixtureBase Fixture;

    protected UnitTestFixture(UnitTestFixtureBase fixture)
    {
        Fixture = fixture;
        fixture.DatabaseId = Guid.NewGuid().ToString();
    }

    protected async Task<RestaurantReadDto> CreateRestaurant(RestaurantCreateDto restaurantCreateDto, DateTime? customDateTime = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        var restaurantsController = Fixture.GetImplementationFromService<RestaurantsController>();
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        return Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
    }

    protected async Task<ProductReadDto> CreateProduct(string restaurantId, DateTime? customDateTime = null, ProductCreateDto? productCreateDto = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        var productsController = Fixture.GetImplementationFromService<ProductsController>();

        var response = await productsController.CreateProduct(restaurantId, productCreateDto ?? ProductMocks.ProductCreateDto);

        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        return Assert.IsType<ProductReadDto>(responseResult.Value);
    }
}
