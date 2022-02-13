using System;
using System.Collections.Generic;
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
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class RestaurantsControllerTest
{
    private ApplicationSecurityToken _restaurantAdminInfo = null!;

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
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
        };

        restaurantCreateDto.OpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DateTime.UtcNow.DayOfWeek,
                StartTimeInMinutes = 0,
                EndTimeInMinutes = 1440,
                StartOrderTimeInMinutes = 0,
                EndOrderTimeInMinutes = 1440
            }
        };

        // Act
        var response = await controller.CreateRestaurant(restaurantCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(_restaurantAdminInfo.UserId);
        responseBody.Email.Should().Be(restaurantCreateDto.Email);
        responseBody.PhoneNumber.Should().Be(restaurantCreateDto.PhoneNumber);
        responseBody.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        responseBody.Name.Should().Be(restaurantCreateDto.Name);
        responseBody.City.Should().Be(restaurantCreateDto.City);
        responseBody.Country.Should().Be(restaurantCreateDto.Country);
        responseBody.StreetName.Should().Be(restaurantCreateDto.StreetName);
        responseBody.ZipCode.Should().Be(restaurantCreateDto.ZipCode);
        responseBody.StreetNumber.Should().Be(restaurantCreateDto.StreetNumber);
        responseBody.IsOpen.Should().Be(restaurantCreateDto.IsOpen);
        responseBody.IsPublic.Should().Be(restaurantCreateDto.IsPublic);
        responseBody.ClosingDates.Should().BeEquivalentTo(restaurantCreateDto.ClosingDates);
        responseBody.OpeningTimes.Should().BeEquivalentTo(restaurantCreateDto.OpeningTimes);
        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);
    }

    // Todo test IsPublished, IsCurrentlyOpenToOrder
}
