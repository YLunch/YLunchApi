using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class RestaurantsControllerTest
{
    private RestaurantsController CreateController()
    {
        return new RestaurantsController(HttpContextAccessorMocker.GetWithAuthorization(TokenMocks.ValidRestaurantAdminAccessToken));
    }

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_201Created()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var response = await controller.CreateRestaurant(RestaurantMocks.RestaurantCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo("works");
    }
}
