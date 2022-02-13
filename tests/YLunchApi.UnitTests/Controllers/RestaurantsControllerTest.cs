using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Application.RestaurantAggregate;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.TestsShared.Models;
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class RestaurantsControllerTest
{
    private ApplicationSecurityToken _restaurantAdminInfo;

    private RestaurantsController CreateController()
    {
        var context = ContextBuilder.BuildContext();
        var restaurantRepository = new RestaurantRepository(context);
        var restaurantService = new RestaurantService(restaurantRepository);
        _restaurantAdminInfo = new ApplicationSecurityToken(TokenMocks.ValidRestaurantAdminAccessToken);
        return new RestaurantsController(
            HttpContextAccessorMocker.GetWithAuthorization(TokenMocks.ValidRestaurantAdminAccessToken),
            restaurantService);
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

        responseBody.Should().BeEquivalentTo(RestaurantMocks.RestaurantReadDto(responseBody.Id),
            options => options
                       .Excluding(x => x.Id)
                       .Excluding(x => x.AdminId)
                       .Excluding(x => x.CreationDateTime));
        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(_restaurantAdminInfo.UserId);
        responseBody.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
