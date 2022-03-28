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
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Controllers;

public class ProductsControllerTest : UnitTestFixture
{
    private readonly ApplicationSecurityToken _restaurantAdminInfo;
    private RestaurantReadDto _restaurant;

    public ProductsControllerTest(UnitTestFixtureBase fixture) : base(fixture)
    {
        _restaurantAdminInfo = new ApplicationSecurityToken(TokenMocks.ValidRestaurantAdminAccessToken);
    }

    #region Utils

    private async Task<RestaurantReadDto> CreateRestaurant(DateTime? customDateTime = null)
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
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        return Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
    }

    private async Task<ProductsController> InitProductsController(DateTime? customDateTime = null)
    {
        _restaurant = await CreateRestaurant(customDateTime);
        return Fixture.GetImplementationFromService<ProductsController>();
    }

    #endregion

    #region CreateProductTests

    [Fact]
    public async Task CreateProduct_Should_Return_A_201Created()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var productsController = await InitProductsController(dateTime);
        var productCreateDto = ProductMocks.ProductCreateDto;

        // Act
        var response = await productsController.CreateProduct(_restaurant.Id, productCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<ProductReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.RestaurantId.Should().Be(_restaurant.Id);
        responseBody.Name.Should().Be(productCreateDto.Name);
        responseBody.Price.Should().Be(productCreateDto.Price);
        responseBody.Description.Should().Be(productCreateDto.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(productCreateDto.Quantity);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeCloseTo(dateTime.AddDays(1), TimeSpan.FromSeconds(5));
        responseBody.Allergens.Should().BeEquivalentTo(productCreateDto.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.Allergens.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_Should_Return_A_409Conflict()
    {
        // Arrange
        var productsController = await InitProductsController();
        var productCreateDto = ProductMocks.ProductCreateDto;

        // Act
        _ = await productsController.CreateProduct(_restaurant.Id, productCreateDto);
        var response = await productsController.CreateProduct(_restaurant.Id, productCreateDto);

        // Assert
        var responseResult = Assert.IsType<ConflictObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.Conflict, $"Product: {productCreateDto.Name} already exists"));
    }

    #endregion

    #region GetRestaurantByIdTests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var productsController = await InitProductsController(dateTime);
        var productCreateDto = ProductMocks.ProductCreateDto;

        var productCreationResponse = await productsController.CreateProduct(_restaurant.Id, productCreateDto);
        var productCreationResponseResult = Assert.IsType<CreatedResult>(productCreationResponse.Result);
        var productCreationResponseBody = Assert.IsType<ProductReadDto>(productCreationResponseResult.Value);

        // Act
        var response = await productsController.GetProductById(productCreationResponseBody.Id);
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<ProductReadDto>(responseResult.Value);

        // Assert
        responseBody.Id.Should().Be(productCreationResponseBody.Id);
        responseBody.RestaurantId.Should().Be(_restaurant.Id);
        responseBody.Name.Should().Be(productCreateDto.Name);
        responseBody.Price.Should().Be(productCreateDto.Price);
        responseBody.Description.Should().Be(productCreateDto.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(productCreateDto.Quantity);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeCloseTo(dateTime.AddDays(1), TimeSpan.FromSeconds(5));
        responseBody.Allergens.Should().BeEquivalentTo(productCreationResponseBody.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
    }

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_404NotFound()
    {
        // Arrange
        var productsController = await InitProductsController();
        var notExistingProductId = Guid.NewGuid().ToString();

        // Act
        var response = await productsController.GetProductById(notExistingProductId);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Product {notExistingProductId} not found"));
    }

    #endregion
}
