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

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_409Conflict()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;

        // Act
        _ = await controller.CreateRestaurant(restaurantCreateDto);
        var response = await controller.CreateRestaurant(restaurantCreateDto);

        // Assert
        var responseResult = Assert.IsType<ConflictObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);
        responseBody.Should().Be("Restaurant already exists");
    }

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        var utcNowMinus2Hours = utcNow.AddHours(-2).Hour;
        var utcNowPlus1Hour = utcNow.AddHours(1).Hour;
        restaurantCreateDto.OpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                StartTimeInMinutes = 0,
                EndTimeInMinutes = 1440,
                StartOrderTimeInMinutes = utcNowMinus2Hours * 60,
                EndOrderTimeInMinutes = utcNowPlus1Hour * 60
            }
        };
        restaurantCreateDto.AddressExtraInformation = "extra information";
        restaurantCreateDto.Base64Logo = "my base 64 encoded logo";
        restaurantCreateDto.Base64Image = "my base 64 encoded image";
        var restaurantCreationResponse = await controller.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await controller.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(_restaurantAdminInfo.UserId);
        responseBody.Email.Should().Be(restaurantCreateDto.Email);
        responseBody.PhoneNumber.Should().Be(restaurantCreateDto.PhoneNumber);
        responseBody.CreationDateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        responseBody.Name.Should().Be(restaurantCreateDto.Name);
        responseBody.City.Should().Be(restaurantCreateDto.City);
        responseBody.Country.Should().Be(restaurantCreateDto.Country);
        responseBody.ZipCode.Should().Be(restaurantCreateDto.ZipCode);
        responseBody.StreetName.Should().Be(restaurantCreateDto.StreetName);
        responseBody.StreetNumber.Should().Be(restaurantCreateDto.StreetNumber);
        responseBody.AddressExtraInformation.Should().Be(restaurantCreateDto.AddressExtraInformation);
        responseBody.IsOpen.Should().Be(restaurantCreateDto.IsOpen);
        responseBody.IsPublic.Should().Be(restaurantCreateDto.IsPublic);
        responseBody.Base64Logo.Should().Be(restaurantCreateDto.Base64Logo);
        responseBody.Base64Image.Should().Be(restaurantCreateDto.Base64Image);
        responseBody.ClosingDates.Should().BeEquivalentTo(restaurantCreateDto.ClosingDates);
        responseBody.OpeningTimes.Should().BeEquivalentTo(restaurantCreateDto.OpeningTimes);
        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);
        responseBody.IsPublished.Should().Be(true);
        responseBody.LastUpdateDateTime.Should().BeNull();
        responseBody.EmailConfirmationDateTime.Should().BeNull();
        responseBody.IsEmailConfirmed.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_OpeningTimes_Empty()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var restaurantCreationResponse = await controller.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await controller.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsPublished.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenToOrder_False_Because_Of_ClosingDates()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = DateTime.UtcNow }
        };
        var restaurantCreationResponse = await controller.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await controller.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsCurrentlyOpenToOrder.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenToOrder_False_Because_Of_OpeningTimes_To_Order()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        var utcNowMinus2Hours = utcNow.AddHours(-3).Hour;
        var utcNowMinus1Hours = utcNow.AddHours(-1).Hour;
        restaurantCreateDto.OpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                StartTimeInMinutes = 0,
                EndTimeInMinutes = 1440,
                StartOrderTimeInMinutes = utcNowMinus2Hours * 60,
                EndOrderTimeInMinutes = utcNowMinus1Hours * 60
            }
        };
        var restaurantCreationResponse = await controller.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await controller.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsCurrentlyOpenToOrder.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenToOrder_False_Because_Of_IsOpen_False()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.IsOpen = false;
        var utcNow = DateTime.UtcNow;
        var utcNowMinus2Hours = utcNow.AddHours(-2).Hour;
        var utcNowPlus1Hour = utcNow.AddHours(1).Hour;
        restaurantCreateDto.OpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                StartTimeInMinutes = 0,
                EndTimeInMinutes = 1440,
                StartOrderTimeInMinutes = utcNowMinus2Hours * 60,
                EndOrderTimeInMinutes = utcNowPlus1Hour * 60
            }
        };
        var restaurantCreationResponse = await controller.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await controller.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsCurrentlyOpenToOrder.Should().Be(false);
    }

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_404NotFound()
    {
        // Arrange
        var controller = CreateController();
        var notExistingRestaurantId = Guid.NewGuid().ToString();

        // Act
        var response = await controller.GetRestaurantById(notExistingRestaurantId);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<string>(responseResult.Value);
        responseBody.Should().Be($"Restaurant {notExistingRestaurantId} not found");
    }

    // Todo test IsPublished
}
