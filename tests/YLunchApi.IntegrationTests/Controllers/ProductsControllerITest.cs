using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.IntegrationTests.Core.Extensions;
using YLunchApi.IntegrationTests.Core.Utils;
using YLunchApi.TestsShared;
using YLunchApi.TestsShared.Mocks;

namespace YLunchApi.IntegrationTests.Controllers;

[Collection("Sequential")]
public class ProductsControllerITest : ControllerITestBase
{
    #region Utils

    private async Task<RestaurantReadDto> CreateRestaurant(RestaurantCreateDto restaurantCreateDto)
    {
        var authenticatedUserInfo = await Authenticate(UserMocks.RestaurantAdminCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var restaurantCreationResponse = await Client.PostAsJsonAsync("restaurants", restaurantCreateDto);
        restaurantCreationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ResponseUtils.DeserializeContentAsync<RestaurantReadDto>(restaurantCreationResponse);
    }

    private async Task<(DateTime, RestaurantReadDto, ProductCreateDto, ProductReadDto)> CreateProduct()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);
        var dateTime = DateTime.UtcNow;
        var productCreateDto = ProductMocks.ProductCreateDto;
        var body = new
        {
            productCreateDto.Name,
            productCreateDto.Price,
            productCreateDto.Quantity,
            productCreateDto.IsActive,
            productCreateDto.ProductType,
            productCreateDto.Image,
            ExpirationDateTime = dateTime.AddDays(1),
            productCreateDto.Description,
            productCreateDto.Allergens,
            productCreateDto.ProductTags
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var productReadDto = await ResponseUtils.DeserializeContentAsync<ProductReadDto>(response);
        return (dateTime, restaurant, productCreateDto, productReadDto);
    }

    #endregion

    #region CreateRestaurant_Tests

    [Fact]
    public async Task CreateProduct_Should_Return_A_201Created()
    {
        // Arrange
        var (dateTime, restaurant, body, responseBody) = await CreateProduct();

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.Name.Should().Be(body.Name);
        responseBody.Price.Should().Be(body.Price);
        responseBody.Description.Should().Be(body.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(body.Quantity);
        responseBody.ProductType.Should().Be(body.ProductType);
        responseBody.Image.Should().Be(body.Image);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeCloseTo(dateTime.AddDays(1), TimeSpan.FromSeconds(5));
        responseBody.Allergens.Should().BeEquivalentTo(body.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.Allergens.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.ProductTags.Should().BeEquivalentTo(body.ProductTags)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.ProductTags.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Missing_Fields()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);
        var body = new
        {
            Name = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("The Name field is required.");
        responseBody.Should().Contain("The Price field is required.");
        responseBody.Should().Contain("The IsActive field is required.");
        responseBody.Should().Contain("The Allergens field is required.");
        responseBody.Should().Contain("The ProductType field is required.");
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Invalid_Fields()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);
        var body = new
        {
            Name = "An invalid Name",
            Description = "An invalid Description",
            Quantity = 0,
            Allergens = new List<dynamic>
            {
                new { BadFieldName = "wrong value" }
            },
            ProductTags = new List<dynamic>
            {
                new { BadFieldName = "wrong value" }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().MatchRegex(@"Name.*Must be lowercase\.");
        responseBody.Should().MatchRegex(@"Description.*Must be lowercase\.");
        responseBody.Should().MatchRegex(@"Quantity.*The field Quantity must be between 1 and 10000\.");
        responseBody.Should().MatchRegex(@"Allergens.*The Name field is required\.");
        responseBody.Should().MatchRegex(@"ProductTags.*The Name field is required\.");
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_400BadRequest_When_Allergens_And_ProductTags_Are_Invalid()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);
        var body = new
        {
            Name = "An invalid Name",
            Description = "An invalid Description",
            Allergens = new List<dynamic>
            {
                new { Name = "An invalid Name" }
            },
            ProductTags = new List<dynamic>
            {
                new { Name = "An invalid Name" }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().MatchRegex(@"Name.*Must be lowercase\.");
        responseBody.Should().MatchRegex(@"Description.*Must be lowercase\.");
        responseBody.Should().MatchRegex(@"Allergens.*Must be lowercase\.");
        responseBody.Should().MatchRegex(@"ProductTags.*Must be lowercase\.");
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_401Unauthorized()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);
        Client.SetAuthorizationHeader(TokenMocks.ExpiredAccessToken);
        var dateTime = DateTime.UtcNow;
        var productCreateDto = ProductMocks.ProductCreateDto;
        var body = new
        {
            productCreateDto.Name,
            productCreateDto.Price,
            productCreateDto.Quantity,
            productCreateDto.IsActive,
            productCreateDto.ProductType,
            ExpirationDateTime = dateTime.AddDays(1),
            productCreateDto.Description,
            productCreateDto.Allergens,
            productCreateDto.ProductTags
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);

        // Assert
        await AssertResponseUtils.AssertUnauthorizedResponse(response);
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_403Forbidden()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);
        var authenticatedUserInfo = await Authenticate(UserMocks.CustomerCreateDto);
        Client.SetAuthorizationHeader(authenticatedUserInfo.AccessToken);
        var dateTime = DateTime.UtcNow;
        var productCreateDto = ProductMocks.ProductCreateDto;
        var body = new
        {
            productCreateDto.Name,
            productCreateDto.Price,
            productCreateDto.Quantity,
            productCreateDto.IsActive,
            productCreateDto.ProductType,
            ExpirationDateTime = dateTime.AddDays(1),
            productCreateDto.Description,
            productCreateDto.Allergens,
            productCreateDto.ProductTags
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurant.Id}/products", body);

        // Assert
        await AssertResponseUtils.AssertForbiddenResponse(response);
    }

    #endregion

    #region GetRestaurantById_Tests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok()
    {
        // Arrange
        var (dateTime, restaurant, body, product) = await CreateProduct();

        // Act
        var response = await Client.GetAsync($"products/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<ProductReadDto>(response);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.Name.Should().Be(body.Name);
        responseBody.Price.Should().Be(body.Price);
        responseBody.Description.Should().Be(body.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(body.Quantity);
        responseBody.ProductType.Should().Be(body.ProductType);
        responseBody.Image.Should().Be(body.Image);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeCloseTo(dateTime.AddDays(1), TimeSpan.FromSeconds(5));
        responseBody.Allergens.Should().BeEquivalentTo(body.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.Allergens.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.ProductTags.Should().BeEquivalentTo(body.ProductTags)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.ProductTags.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
    }

    #endregion
}
