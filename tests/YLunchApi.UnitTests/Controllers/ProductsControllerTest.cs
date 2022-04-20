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
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Controllers;

public class ProductsControllerTest : UnitTestFixture
{
    public ProductsControllerTest(UnitTestFixtureBase fixture) : base(fixture)
    {
    }

    #region Utils

    private ProductsController InitProductsController(DateTime? customDateTime = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        return Fixture.GetImplementationFromService<ProductsController>();
    }

    #endregion

    #region CreateProductTests

    [Fact]
    public async Task CreateProduct_Should_Return_A_201Created()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var productCreateDto = ProductMocks.ProductCreateDto;
        productCreateDto.ExpirationDateTime = dateTime.AddDays(1);

        // Act
        var response = await productsController.CreateProduct(restaurant.Id, productCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<ProductReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.Name.Should().Be(productCreateDto.Name);
        responseBody.Price.Should().Be(productCreateDto.Price);
        responseBody.Description.Should().Be(productCreateDto.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(productCreateDto.Quantity);
        responseBody.ProductType.Should().Be(productCreateDto.ProductType);
        responseBody.Image.Should().Be(productCreateDto.Image);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeCloseTo(dateTime.AddDays(1), TimeSpan.FromSeconds(5));
        responseBody.Allergens.Should().BeEquivalentTo(productCreateDto.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.Allergens.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.ProductTags.Should().BeEquivalentTo(productCreateDto.ProductTags)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.ProductTags.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_Should_Return_A_201Created_When_A_Product_With_Same_Name_Already_Exists_But_In_A_Different_Restaurant()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.Name = "other restaurant";
        var otherRestaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, restaurantCreateDto, dateTime);
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var productCreateDto = ProductMocks.ProductCreateDto;

        // Act
        _ = await productsController.CreateProduct(otherRestaurant.Id, productCreateDto);
        var response = await productsController.CreateProduct(restaurant.Id, productCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<ProductReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.Name.Should().Be(productCreateDto.Name);
        responseBody.Price.Should().Be(productCreateDto.Price);
        responseBody.Description.Should().Be(productCreateDto.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(productCreateDto.Quantity);
        responseBody.ProductType.Should().Be(productCreateDto.ProductType);
        responseBody.Image.Should().Be(productCreateDto.Image);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeNull();
        responseBody.Allergens.Should().BeEquivalentTo(productCreateDto.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.Allergens.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.ProductTags.Should().BeEquivalentTo(productCreateDto.ProductTags)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.ProductTags.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_Should_Return_A_409Conflict()
    {
        // Arrange
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, DateTimeMocks.Monday20220321T1000Utc);
        var productsController = InitProductsController();
        var productCreateDto = ProductMocks.ProductCreateDto;

        // Act
        _ = await productsController.CreateProduct(restaurant.Id, productCreateDto);
        var response = await productsController.CreateProduct(restaurant.Id, productCreateDto);

        // Assert
        var responseResult = Assert.IsType<ConflictObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.Conflict, $"Product: {productCreateDto.Name} already exists."));
    }

    [Fact]
    public async Task CreateProduct_Should_Return_A_404NotFound()
    {
        // Arrange
        var productsController = InitProductsController();
        var productCreateDto = ProductMocks.ProductCreateDto;

        // Act
        var response = await productsController.CreateProduct("NON_EXISTENT_RESTAURANT_ID", productCreateDto);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, "Restaurant: NON_EXISTENT_RESTAURANT_ID not found."));
    }

    [Fact]
    public async Task CreateProduct_Should_Return_A_201Created_And_Allergens_And_ProductTags_Should_Not_Be_Duplicated_When_They_Already_Exist()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var initialProductCreateDto = ProductMocks.ProductCreateDto;
        initialProductCreateDto.Name = "first pizza";

        var initialProductCreationResponse = await productsController.CreateProduct(restaurant.Id, initialProductCreateDto);
        var initialProductCreationResponseResult = Assert.IsType<CreatedResult>(initialProductCreationResponse.Result);
        var initialProductCreationResponseBody = Assert.IsType<ProductReadDto>(initialProductCreationResponseResult.Value);

        var existingAllergens = initialProductCreationResponseBody.Allergens;
        var existingProductTags = initialProductCreationResponseBody.ProductTags;

        var productCreateDto = ProductMocks.ProductCreateDto;

        // Act
        var response = await productsController.CreateProduct(restaurant.Id, productCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<ProductReadDto>(responseResult.Value);

        responseBody.Allergens.Should().BeEquivalentTo(existingAllergens);
        responseBody.ProductTags.Should().BeEquivalentTo(existingProductTags);
    }

    #endregion

    #region GetRestaurantByIdTests

    [Fact]
    public async Task GetProductById_Should_Return_A_200Ok()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var productCreateDto = ProductMocks.ProductCreateDto;
        productCreateDto.ExpirationDateTime = dateTime.AddDays(1);

        var productCreationResponse = await productsController.CreateProduct(restaurant.Id, productCreateDto);
        var productCreationResponseResult = Assert.IsType<CreatedResult>(productCreationResponse.Result);
        var productCreationResponseBody = Assert.IsType<ProductReadDto>(productCreationResponseResult.Value);

        // Act
        var response = await productsController.GetProductById(productCreationResponseBody.Id);
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<ProductReadDto>(responseResult.Value);

        // Assert
        responseBody.Id.Should().Be(productCreationResponseBody.Id);
        responseBody.RestaurantId.Should().Be(restaurant.Id);
        responseBody.Name.Should().Be(productCreateDto.Name);
        responseBody.Price.Should().Be(productCreateDto.Price);
        responseBody.Description.Should().Be(productCreateDto.Description);
        responseBody.IsActive.Should().Be(true);
        responseBody.Quantity.Should().Be(productCreateDto.Quantity);
        responseBody.ProductType.Should().Be(productCreateDto.ProductType);
        responseBody.Image.Should().Be(productCreateDto.Image);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
        responseBody.ExpirationDateTime.Should().BeCloseTo(dateTime.AddDays(1), TimeSpan.FromSeconds(5));
        responseBody.Allergens.Should().BeEquivalentTo(productCreationResponseBody.Allergens)
                    .And
                    .BeInAscendingOrder(x => x.Name);
        responseBody.ProductTags.Should().BeEquivalentTo(productCreationResponseBody.ProductTags)
                    .And
                    .BeInAscendingOrder(x => x.Name);
    }

    [Fact]
    public async Task GetProductById_Should_Return_A_404NotFound()
    {
        // Arrange
        var productsController = InitProductsController();
        var notExistingProductId = Guid.NewGuid().ToString();

        // Act
        var response = await productsController.GetProductById(notExistingProductId);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Product: {notExistingProductId} not found."));
    }

    #endregion

    #region GetProductsTests

    [Fact]
    public async Task GetProducts_Should_Return_A_200Ok()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);

        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.ExpirationDateTime = dateTime.AddDays(1);
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var expectedProducts = new List<ProductReadDto>
        {
            product1,
            product2
        };

        // Act
        var response = await productsController.GetProductsByRestaurantId(restaurant.Id);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<ProductReadDto>>(responseResult.Value);
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
            actualProduct.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            _ = expectedProduct.ExpirationDateTime switch
            {
                null => actualProduct.ExpirationDateTime.Should().BeNull(),
                { } dt => actualProduct.ExpirationDateTime.Should().BeCloseTo(dt, TimeSpan.FromSeconds(5))
            };
            actualProduct.Allergens.Should().BeEquivalentTo(expectedProduct.Allergens)
                         .And
                         .BeInAscendingOrder(x => x.Name);
            actualProduct.ProductTags.Should().BeEquivalentTo(expectedProduct.ProductTags)
                         .And
                         .BeInAscendingOrder(x => x.Name);
        }
    }

    [Fact]
    public async Task GetProducts_With_Pagination_Should_Return_A_200Ok_With_Correct_Products()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        productCreateDto4.ExpirationDateTime = dateTime.AddDays(1);
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto5, dateTime);

        var expectedProducts = new List<ProductReadDto>
        {
            product3,
            product4
        };

        var filter = new ProductFilter
        {
            Page = 2,
            Size = 2
        };

        // Act
        var response = await productsController.GetProductsByRestaurantId(restaurant.Id, filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<ProductReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        responseBody[0].Name.Should().Be("product3");
        responseBody[1].Name.Should().Be("product4");

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
            actualProduct.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            _ = expectedProduct.ExpirationDateTime switch
            {
                null => actualProduct.ExpirationDateTime.Should().BeNull(),
                { } dt => actualProduct.ExpirationDateTime.Should().BeCloseTo(dt, TimeSpan.FromSeconds(5))
            };
            actualProduct.Allergens.Should().BeEquivalentTo(expectedProduct.Allergens)
                         .And
                         .BeInAscendingOrder(x => x.Name);
            actualProduct.ProductTags.Should().BeEquivalentTo(expectedProduct.ProductTags)
                         .And
                         .BeInAscendingOrder(x => x.Name);
        }
    }

    [Fact]
    public async Task GetProducts_With_Filter_IsAvailable_True_Should_Return_A_200Ok_With_Correct_Products()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.Quantity = 0;
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        productCreateDto2.IsActive = false;
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        productCreateDto1.Quantity = null;
        var product3 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        productCreateDto4.ExpirationDateTime = dateTime.AddDays(-1);
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        productCreateDto5.ExpirationDateTime = dateTime.AddDays(1);
        var product5 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto5, dateTime);

        var expectedProducts = new List<ProductReadDto>
        {
            product3,
            product5
        };

        var filter = new ProductFilter
        {
            IsAvailable = true
        };

        // Act
        var response = await productsController.GetProductsByRestaurantId(restaurant.Id, filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<ProductReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        responseBody[0].Name.Should().Be("product3");
        responseBody[1].Name.Should().Be("product5");

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
            actualProduct.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            _ = expectedProduct.ExpirationDateTime switch
            {
                null => actualProduct.ExpirationDateTime.Should().BeNull(),
                { } dt => actualProduct.ExpirationDateTime.Should().BeCloseTo(dt, TimeSpan.FromSeconds(5))
            };
            actualProduct.Allergens.Should().BeEquivalentTo(expectedProduct.Allergens)
                         .And
                         .BeInAscendingOrder(x => x.Name);
            actualProduct.ProductTags.Should().BeEquivalentTo(expectedProduct.ProductTags)
                         .And
                         .BeInAscendingOrder(x => x.Name);
        }
    }

    [Fact]
    public async Task GetProducts_With_Filter_IsAvailable_False_Should_Return_A_200Ok_With_Correct_Products()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.Quantity = 0;
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        productCreateDto2.IsActive = false;
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        productCreateDto1.Quantity = null;
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        productCreateDto4.ExpirationDateTime = dateTime.AddDays(-1);
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        productCreateDto5.ExpirationDateTime = dateTime.AddDays(1);
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto5, dateTime);

        var expectedProducts = new List<ProductReadDto>
        {
            product1,
            product2,
            product4
        };

        var filter = new ProductFilter
        {
            IsAvailable = false
        };

        // Act
        var response = await productsController.GetProductsByRestaurantId(restaurant.Id, filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<ProductReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(3);

        responseBody[0].Name.Should().Be("product1");
        responseBody[1].Name.Should().Be("product2");
        responseBody[2].Name.Should().Be("product4");

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualProduct = responseBody[i];
            var expectedProduct = expectedProducts[i];

            actualProduct.Id.Should().Be(expectedProduct.Id);
            actualProduct.RestaurantId.Should().Be(restaurant.Id);
            actualProduct.Name.Should().Be(expectedProduct.Name);
            actualProduct.Price.Should().Be(expectedProduct.Price);
            actualProduct.Description.Should().Be(expectedProduct.Description);
            actualProduct.IsActive.Should().Be(expectedProduct.IsActive);
            actualProduct.Quantity.Should().Be(expectedProduct.Quantity);
            actualProduct.ProductType.Should().Be(expectedProduct.ProductType);
            actualProduct.Image.Should().Be(expectedProduct.Image);
            actualProduct.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            _ = expectedProduct.ExpirationDateTime switch
            {
                null => actualProduct.ExpirationDateTime.Should().BeNull(),
                { } dt => actualProduct.ExpirationDateTime.Should().BeCloseTo(dt, TimeSpan.FromSeconds(5))
            };
            actualProduct.Allergens.Should().BeEquivalentTo(expectedProduct.Allergens)
                         .And
                         .BeInAscendingOrder(x => x.Name);
            actualProduct.ProductTags.Should().BeEquivalentTo(expectedProduct.ProductTags)
                         .And
                         .BeInAscendingOrder(x => x.Name);
        }
    }

    [Fact]
    public async Task GetProducts_With_Filter_ProductIds_Should_Return_A_200Ok_With_Correct_Products()
    {
        // Arrange
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var restaurant = await CreateRestaurant(TokenMocks.ValidRestaurantAdminAccessToken, RestaurantMocks.SimpleRestaurantCreateDto, dateTime);
        var productsController = InitProductsController(dateTime);
        var productCreateDto1 = ProductMocks.ProductCreateDto;
        productCreateDto1.Name = "product1";
        productCreateDto1.Quantity = 0;
        var product1 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto1, dateTime);

        var productCreateDto2 = ProductMocks.ProductCreateDto;
        productCreateDto2.Name = "product2";
        productCreateDto2.IsActive = false;
        var product2 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto2, dateTime);

        var productCreateDto3 = ProductMocks.ProductCreateDto;
        productCreateDto3.Name = "product3";
        productCreateDto1.Quantity = null;
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto3, dateTime);

        var productCreateDto4 = ProductMocks.ProductCreateDto;
        productCreateDto4.Name = "product4";
        productCreateDto4.ExpirationDateTime = dateTime.AddDays(-1);
        var product4 = await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto4, dateTime);

        var productCreateDto5 = ProductMocks.ProductCreateDto;
        productCreateDto5.Name = "product5";
        productCreateDto5.ExpirationDateTime = dateTime.AddDays(1);
        await CreateProduct(TokenMocks.ValidRestaurantAdminAccessToken, restaurant.Id, productCreateDto5, dateTime);

        var expectedProducts = new List<ProductReadDto>
        {
            product1,
            product2,
            product4
        };

        var filter = new ProductFilter
        {
            ProductIds = new SortedSet<string>
            {
                product1.Id,
                product2.Id,
                product4.Id
            }
        };

        // Act
        var response = await productsController.GetProductsByRestaurantId(restaurant.Id, filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<ProductReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(3);

        responseBody[0].Name.Should().Be("product1");
        responseBody[1].Name.Should().Be("product2");
        responseBody[2].Name.Should().Be("product4");

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualProduct = responseBody[i];
            var expectedProduct = expectedProducts[i];

            actualProduct.Id.Should().Be(expectedProduct.Id);
            actualProduct.RestaurantId.Should().Be(restaurant.Id);
            actualProduct.Name.Should().Be(expectedProduct.Name);
            actualProduct.Price.Should().Be(expectedProduct.Price);
            actualProduct.Description.Should().Be(expectedProduct.Description);
            actualProduct.IsActive.Should().Be(expectedProduct.IsActive);
            actualProduct.Quantity.Should().Be(expectedProduct.Quantity);
            actualProduct.ProductType.Should().Be(expectedProduct.ProductType);
            actualProduct.Image.Should().Be(expectedProduct.Image);
            actualProduct.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            _ = expectedProduct.ExpirationDateTime switch
            {
                null => actualProduct.ExpirationDateTime.Should().BeNull(),
                { } dt => actualProduct.ExpirationDateTime.Should().BeCloseTo(dt, TimeSpan.FromSeconds(5))
            };
            actualProduct.Allergens.Should().BeEquivalentTo(expectedProduct.Allergens)
                         .And
                         .BeInAscendingOrder(x => x.Name);
            actualProduct.ProductTags.Should().BeEquivalentTo(expectedProduct.ProductTags)
                         .And
                         .BeInAscendingOrder(x => x.Name);
        }
    }

    [Fact]
    public async Task GetProducts_Should_Return_A_404NotFound_When_Restaurant_Not_Found()
    {
        // Assert
        var dateTime = DateTimeMocks.Monday20220321T1000Utc;
        var productsController = InitProductsController(dateTime);

        // Act
        var response = await productsController.GetProductsByRestaurantId("NON_EXISTENT_RESTAURANT_ID");

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound, "Restaurant: NON_EXISTENT_RESTAURANT_ID not found."));
    }

    #endregion
}
