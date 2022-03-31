using System.Threading.Tasks;
using FluentAssertions;
using Mapster;
using Moq;
using Xunit;
using YLunchApi.Domain.CommonAggregate.Services;
using YLunchApi.Domain.RestaurantAggregate.Filters;
using YLunchApi.Domain.RestaurantAggregate.Models;
using YLunchApi.Domain.RestaurantAggregate.Services;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;

namespace YLunchApi.UnitTests.Infrastructure;

public class ProductRepositoryTest : UnitTestFixture
{
    public ProductRepositoryTest(UnitTestFixtureBase fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetProducts_Should_Return_Corrects_Products_When_RestaurantId_In_Filter_Is_Null()
    {
        // Arrange
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTimeMocks.Monday20220321T1000Utc);
        Fixture.InitFixture(configuration => configuration.DateTimeProvider = dateTimeProviderMock.Object);

        var dateTimeProvider = Fixture.GetImplementationFromService<IDateTimeProvider>();
        var productRepository = Fixture.GetImplementationFromService<IProductRepository>();

        var product1 = ProductMocks.ProductCreateDto.Adapt<Product>();
        product1.Name = "product1";
        product1.CreationDateTime = dateTimeProvider.UtcNow;
        product1.RestaurantId = "restaurantId1";
        await productRepository.Create(product1);

        var product2 = ProductMocks.ProductCreateDto.Adapt<Product>();
        product2.Name = "product2";
        product2.CreationDateTime = dateTimeProvider.UtcNow;
        product2.RestaurantId = "restaurantId1";
        await productRepository.Create(product2);

        var product3 = ProductMocks.ProductCreateDto.Adapt<Product>();
        product3.Name = "product3";
        product3.CreationDateTime = dateTimeProvider.UtcNow;
        product3.RestaurantId = "restaurantId2";
        await productRepository.Create(product3);

        // Act
        var products = await productRepository.GetProducts(new ProductFilter { RestaurantId = null });

        // Assert
        products.Count.Should().Be(3);
    }
}
