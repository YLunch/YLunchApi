using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Application.RestaurantAggregate;
using YLunchApi.Authentication.Models;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.Core.Utils;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Helpers.Extensions;
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

    #region CreateRestaurantTests

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_201Created()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;

        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = DateTime.Parse("2021-12-31") },
            new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
        };

        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.AddDays(-1).DayOfWeek,
                OffsetInMinutes = utcNow.MinutesFromMidnight(),
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNow.MinutesFromMidnight(),
                DurationInMinutes = 2 * 60
            }
        };

        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.AddDays(-1).DayOfWeek,
                OffsetInMinutes = utcNow.MinutesFromMidnight(),
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNow.MinutesFromMidnight(),
                DurationInMinutes = 2 * 60
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

        responseBody.ClosingDates.Should().BeEquivalentTo(restaurantCreateDto.ClosingDates)
                    .And
                    .BeInAscendingOrder(x => x.ClosingDateTime);

        responseBody.PlaceOpeningTimes.Should().BeEquivalentTo(
            OpeningTimeUtils.AscendingOrder(restaurantCreateDto.PlaceOpeningTimes),
            options => options.WithStrictOrdering());
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenInPlace.Should().Be(true);

        responseBody.OrderOpeningTimes.Should().BeEquivalentTo(
            OpeningTimeUtils.AscendingOrder(restaurantCreateDto.OrderOpeningTimes),
            options => options.WithStrictOrdering());
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == responseBody.Id)
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);

        responseBody.IsPublished.Should().Be(true);
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
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new MessageDto("Restaurant already exists"));
    }

    #endregion

    #region GetRestaurantByIdTests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        var dateTimeBeforeUtcNow = utcNow.AddHours(-2);
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DateTime.UtcNow.DayOfWeek,
                OffsetInMinutes = 0,
                DurationInMinutes = 1439
            }
        };
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DateTime.UtcNow.DayOfWeek,
                OffsetInMinutes = dateTimeBeforeUtcNow.MinutesFromMidnight(),
                DurationInMinutes = 2 * 60
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
        responseBody.PlaceOpeningTimes.Should().BeEquivalentTo(restaurantCreateDto.PlaceOpeningTimes);
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == restaurantId).Should()
                    .BeTrue();
        responseBody.PlaceOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenInPlace.Should().Be(true);
        responseBody.OrderOpeningTimes.Should().BeEquivalentTo(restaurantCreateDto.OrderOpeningTimes);
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && x.RestaurantId == restaurantId).Should()
                    .BeTrue();
        responseBody.OrderOpeningTimes.Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                    .Should().BeTrue();
        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);
        responseBody.IsPublished.Should().Be(true);
        responseBody.LastUpdateDateTime.Should().BeNull();
        responseBody.EmailConfirmationDateTime.Should().BeNull();
        responseBody.IsEmailConfirmed.Should().Be(false);
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
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new MessageDto($"Restaurant {notExistingRestaurantId} not found"));
    }

    #endregion

    #region IsPublishedTests

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_Name()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.Name = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_Email()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.Email = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_PhoneNumber()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.PhoneNumber = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_Country()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.Country = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_City()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.City = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_ZipCode()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.ZipCode = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_StreetName()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.StreetName = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_StreetNumber()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.StreetNumber = "";
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_IsPublic_False()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.IsPublic = false;
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_No_OpeningTimes()
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

    // Todo test IsPublished when product is active

    #endregion

    #region IsCurrentlyOpenInPlaceTests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_True_Even_Day_After()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.AddDays(-1).DayOfWeek,
                OffsetInMinutes = 1380, // 23H00
                DurationInMinutes = utcNow.MinutesFromMidnight() + 60
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

        responseBody.IsCurrentlyOpenInPlace.Should().Be(true);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_ClosingDates()
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

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_Day_Out_Of_OpeningTimes_To_Order()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var tomorrowDateTime = DateTime.UtcNow.AddDays(1);
        var tomorrowDateTimeMinus1H = tomorrowDateTime.AddHours(-1);
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = tomorrowDateTime.DayOfWeek,
                OffsetInMinutes = tomorrowDateTimeMinus1H.MinutesFromMidnight(),
                DurationInMinutes = 60
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

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_Before_OpeningTimes_To_Order()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        var utcNowPlus3H = utcNow.AddHours(3);
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNowPlus3H.MinutesFromMidnight(),
                DurationInMinutes = 120
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

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_After_OpeningTimes_To_Order()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        var utcNowMinus3H = utcNow.AddHours(-3);
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNowMinus3H.MinutesFromMidnight(),
                DurationInMinutes = 120
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

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_IsOpen_False()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        restaurantCreateDto.IsOpen = false;
        var utcNow = DateTime.UtcNow;
        var utcNowMinus2H = utcNow.AddHours(-2);
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNowMinus2H.MinutesFromMidnight(),
                DurationInMinutes = 180
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

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    #endregion

    #region IsCurrentlyOpenToOrderTests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenToOrder_True_Even_Day_After()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.AddDays(-1).DayOfWeek,
                OffsetInMinutes = 1380, // 23H00
                DurationInMinutes = utcNow.MinutesFromMidnight() + 60
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

        responseBody.IsCurrentlyOpenToOrder.Should().Be(true);
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenToOrder_False_Because_Of_Day_Out_Of_OpeningTimes_To_Order()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var tomorrowDateTime = DateTime.UtcNow.AddDays(1);
        var tomorrowDateTimeMinus1H = tomorrowDateTime.AddHours(-1);
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = tomorrowDateTime.DayOfWeek,
                OffsetInMinutes = tomorrowDateTimeMinus1H.MinutesFromMidnight(),
                DurationInMinutes = 60
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenToOrder_False_Because_Of_Before_OpeningTimes_To_Order()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        var utcNowPlus3H = utcNow.AddHours(3);
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNowPlus3H.MinutesFromMidnight(),
                DurationInMinutes = 120
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
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenToOrder_False_Because_Of_After_OpeningTimes_To_Order()
    {
        // Arrange
        var controller = CreateController();
        var restaurantCreateDto = RestaurantMocks.RestaurantCreateDto;
        var utcNow = DateTime.UtcNow;
        var utcNowMinus3H = utcNow.AddHours(-3);
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNowMinus3H.MinutesFromMidnight(),
                DurationInMinutes = 120
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
        var utcNowMinus2H = utcNow.AddHours(-2);
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = utcNow.DayOfWeek,
                OffsetInMinutes = utcNowMinus2H.MinutesFromMidnight(),
                DurationInMinutes = 180
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

    #endregion
}
