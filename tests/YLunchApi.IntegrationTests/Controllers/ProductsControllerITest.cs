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

    private async Task<ProductReadDto> CreateProduct(string restaurantId, ProductCreateDto productCreateDto)
    {
        // Arrange
        var body = new
        {
            productCreateDto.Name,
            productCreateDto.Price,
            productCreateDto.Quantity,
            productCreateDto.IsActive,
            productCreateDto.ProductType,
            productCreateDto.Image,
            productCreateDto.ExpirationDateTime,
            productCreateDto.Description,
            productCreateDto.Allergens,
            productCreateDto.ProductTags
        };

        // Act
        var response = await Client.PostAsJsonAsync($"restaurants/{restaurantId}/products", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var productReadDto = await ResponseUtils.DeserializeContentAsync<ProductReadDto>(response);
        return productReadDto;
    }

    private async Task<(DateTime, RestaurantReadDto, ProductCreateDto, ProductReadDto)> CreateRestaurantAndProduct()
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

    #region CreateRestaurantTests

    [Fact]
    public async Task CreateProduct_Should_Return_A_201Created()
    {
        // Arrange
        var (dateTime, restaurant, body, responseBody) = await CreateRestaurantAndProduct();

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

    #region GetRestaurantByIdTests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok()
    {
        // Arrange
        var (dateTime, restaurant, body, product) = await CreateRestaurantAndProduct();

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

    #region GetProductsTests

    [Fact]
    public async Task GetProducts_Should_Return_A_200Ok()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.ExpirationDateTime = DateTime.UtcNow.AddDays(1);
        var product1 = await CreateProduct(restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        productCreateDto2.ExpirationDateTime = DateTime.UtcNow.AddDays(1);
        var product2 = await CreateProduct(restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        productCreateDto3.ExpirationDateTime = DateTime.UtcNow.AddDays(1);
        var product3 = await CreateProduct(restaurant.Id, productCreateDto3);

        var expectedProducts = new List<ProductReadDto>
        {
            product1,
            product2,
            product3
        };

        // Act
        var response = await Client.GetAsync($"restaurants/{restaurant.Id}/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<ProductReadDto>>(response);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualProduct = responseBody[i];
            var expectedProduct = expectedProducts[i];

            actualProduct.Id.Should().Be(expectedProduct.Id);
            actualProduct.RestaurantId.Should().Be(restaurant.Id);
            actualProduct.Name.Should().Be(expectedProduct.Name);
            actualProduct.Price.Should().Be(expectedProduct.Price);
            actualProduct.Description.Should().Be(expectedProduct.Description);
            actualProduct.IsActive.Should().Be(true);
            actualProduct.Quantity.Should().Be(expectedProduct.Quantity);
            actualProduct.ProductType.Should().Be(expectedProduct.ProductType);
            actualProduct.Image.Should().Be(expectedProduct.Image);
            actualProduct.CreationDateTime.Should().BeCloseTo(expectedProduct.CreationDateTime, TimeSpan.FromSeconds(5));
            actualProduct.ExpirationDateTime.Should().BeCloseTo((DateTime)expectedProduct.ExpirationDateTime!, TimeSpan.FromSeconds(5));
            actualProduct.Allergens.Should().BeEquivalentTo(expectedProduct.Allergens)
                         .And
                         .BeInAscendingOrder(x => x.Name);
            actualProduct.ProductTags.Should().BeEquivalentTo(expectedProduct.ProductTags)
                         .And
                         .BeInAscendingOrder(x => x.Name);
        }
    }

    [Fact]
    public async Task GetProducts_Should_Return_A_200Ok_With_Correct_Products()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.ExpirationDateTime = DateTime.UtcNow.AddDays(1);
        productCreateDto1.IsActive = false;
        await CreateProduct(restaurant.Id, productCreateDto1);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        productCreateDto2.ExpirationDateTime = DateTime.UtcNow.AddDays(1);
        productCreateDto2.IsActive = false;
        await CreateProduct(restaurant.Id, productCreateDto2);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        productCreateDto3.ExpirationDateTime = DateTime.UtcNow.AddDays(1);
        var product3 = await CreateProduct(restaurant.Id, productCreateDto3);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        productCreateDto4.ExpirationDateTime = DateTime.UtcNow.AddDays(1);
        productCreateDto4.Quantity = null;
        var product4 = await CreateProduct(restaurant.Id, productCreateDto4);

        var expectedProducts = new List<ProductReadDto>
        {
            product3,
            product4
        };

        // Act
        var response = await Client.GetAsync($"restaurants/{restaurant.Id}/products?page=2&size=2&isAvailable=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ResponseUtils.DeserializeContentAsync<List<ProductReadDto>>(response);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualProduct = responseBody[i];
            var expectedProduct = expectedProducts[i];

            actualProduct.Id.Should().Be(expectedProduct.Id);
            actualProduct.RestaurantId.Should().Be(restaurant.Id);
            actualProduct.Name.Should().Be(expectedProduct.Name);
            actualProduct.Price.Should().Be(expectedProduct.Price);
            actualProduct.Description.Should().Be(expectedProduct.Description);
            actualProduct.IsActive.Should().Be(true);
            actualProduct.Quantity.Should().Be(expectedProduct.Quantity);
            actualProduct.ProductType.Should().Be(expectedProduct.ProductType);
            actualProduct.Image.Should().Be(expectedProduct.Image);
            actualProduct.CreationDateTime.Should().BeCloseTo(expectedProduct.CreationDateTime, TimeSpan.FromSeconds(5));
            actualProduct.ExpirationDateTime.Should().BeCloseTo((DateTime)expectedProduct.ExpirationDateTime!, TimeSpan.FromSeconds(5));
            actualProduct.Allergens.Should().BeEquivalentTo(expectedProduct.Allergens)
                         .And
                         .BeInAscendingOrder(x => x.Name);
            actualProduct.ProductTags.Should().BeEquivalentTo(expectedProduct.ProductTags)
                         .And
                         .BeInAscendingOrder(x => x.Name);
        }
    }

    [Fact]
    public async Task GetProducts_Should_Return_A_400BadRequest_When_Invalid_Filter()
    {
        // Arrange
        var restaurant = await CreateRestaurant(RestaurantMocks.SimpleRestaurantCreateDto);

        // Act
        var response = await Client.GetAsync($"restaurants/{restaurant.Id}/products?page=0&size=51&isAvailable=25");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await ResponseUtils.DeserializeContentAsync(response);

        responseBody.Should().Contain("Page must be an integer between 1 and 100000.");
        responseBody.Should().Contain("Size must be an integer between 1 and 50.");
        responseBody.Should().Contain("The value '25' is not valid for IsAvailable.");
    }

    #endregion
}
