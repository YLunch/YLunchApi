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

public class RestaurantsControllerTest : UnitTestFixture
{
    private readonly ApplicationSecurityToken _restaurantAdminInfo;

    public RestaurantsControllerTest(UnitTestFixtureBase fixture) : base(fixture)
    {
        _restaurantAdminInfo = new ApplicationSecurityToken(TokenMocks.ValidRestaurantAdminAccessToken);
    }

    private RestaurantsController InitRestaurantsController(DateTime? customDateTime = null)
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(customDateTime ?? DateTime.UtcNow);
        Fixture.InitFixture(configuration =>
        {
            configuration.AccessToken = TokenMocks.ValidRestaurantAdminAccessToken;
            configuration.DateTimeProvider = dateTimeProviderMock.Object;
        });
        return Fixture.GetImplementationFromService<RestaurantsController>();
    }

    #region CreateRestaurantTests

    [Fact]
    public async Task CreateRestaurant_Should_Return_A_201Created()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);

        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;

        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = DateTime.Parse("2021-12-31") },
            new() { ClosingDateTime = DateTime.Parse("2021-12-25") }
        };

        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Sunday,
                OffsetInMinutes = 10 * 60,
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 10 * 60,
                DurationInMinutes = 2 * 60
            }
        };

        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Sunday,
                OffsetInMinutes = 10 * 60,
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 10 * 60,
                DurationInMinutes = 2 * 60
            }
        };

        // Act
        var response = await restaurantsController.CreateRestaurant(restaurantCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(_restaurantAdminInfo.UserId);
        responseBody.Email.Should().Be(restaurantCreateDto.Email);
        responseBody.PhoneNumber.Should().Be(restaurantCreateDto.PhoneNumber);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;

        // Act
        _ = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var response = await restaurantsController.CreateRestaurant(restaurantCreateDto);

        // Assert
        var responseResult = Assert.IsType<ConflictObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should().BeEquivalentTo(new ErrorDto(HttpStatusCode.Conflict, "Restaurant already exists"));
    }

    #endregion

    #region GetRestaurantByIdTests

    [Fact]
    public async Task GetRestaurantById_Should_Return_A_200Ok()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);

        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;

        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 0,
                DurationInMinutes = 1439
            }
        };
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 8 * 60,
                DurationInMinutes = 2 * 60
            }
        };
        restaurantCreateDto.AddressExtraInformation = "extra information";
        restaurantCreateDto.Base64Logo = "my base 64 encoded logo";
        restaurantCreateDto.Base64Image = "my base 64 encoded image";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.Id.Should().MatchRegex(GuidUtils.Regex);
        responseBody.AdminId.Should().Be(_restaurantAdminInfo.UserId);
        responseBody.Email.Should().Be(restaurantCreateDto.Email);
        responseBody.PhoneNumber.Should().Be(restaurantCreateDto.PhoneNumber);
        responseBody.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
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
        var restaurantsController = InitRestaurantsController();
        var notExistingRestaurantId = Guid.NewGuid().ToString();

        // Act
        var response = await restaurantsController.GetRestaurantById(notExistingRestaurantId);

        // Assert
        var responseResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        var responseBody = Assert.IsType<ErrorDto>(responseResult.Value);
        responseBody.Should()
                    .BeEquivalentTo(new ErrorDto(HttpStatusCode.NotFound,
                        $"Restaurant {notExistingRestaurantId} not found"));
    }

    #endregion

    #region IsPublishedTests

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsPublished_False_Because_Of_Missing_Name()
    {
        // Arrange
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.Name = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.Email = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.PhoneNumber = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.Country = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.City = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.ZipCode = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.StreetName = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.StreetNumber = "";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.IsPublic = false;
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var restaurantsController = InitRestaurantsController();
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-20T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Saturday,
                OffsetInMinutes = 23 * 60,
                DurationInMinutes = 11 * 60
            }
        };
        restaurantCreateDto.AddressExtraInformation = "extra information";
        restaurantCreateDto.Base64Logo = "my base 64 encoded logo";
        restaurantCreateDto.Base64Image = "my base 64 encoded image";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = dateTime }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_Day_Out_Of_OpeningTimes_In_Place()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Tuesday,
                OffsetInMinutes = 9 * 60,
                DurationInMinutes = 2 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_Before_OpeningTimes_In_Place()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 13 * 60,
                DurationInMinutes = 2 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsCurrentlyOpenInPlace.Should().Be(false);
    }

    [Fact]
    public async Task
        GetRestaurantById_Should_Return_A_200Ok_Having_IsCurrentlyOpenInPlace_False_Because_Of_After_OpeningTimes_In_Place()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 8 * 60,
                DurationInMinutes = 1 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.IsOpen = false;
        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 9 * 60,
                DurationInMinutes = 3 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Sunday,
                OffsetInMinutes = 23 * 60,
                DurationInMinutes = 12 * 60
            }
        };
        restaurantCreateDto.AddressExtraInformation = "extra information";
        restaurantCreateDto.Base64Logo = "my base 64 encoded logo";
        restaurantCreateDto.Base64Image = "my base 64 encoded image";
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = dateTime }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Tuesday,
                OffsetInMinutes = 9 * 60,
                DurationInMinutes = 2 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 13 * 60,
                DurationInMinutes = 2 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 7 * 60,
                DurationInMinutes = 2 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

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
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.IsOpen = false;
        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = DayOfWeek.Monday,
                OffsetInMinutes = 8 * 60,
                DurationInMinutes = 3 * 60
            }
        };
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        var restaurantCreationResponseBody = Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
        var restaurantId = restaurantCreationResponseBody.Id;

        // Act
        var response = await restaurantsController.GetRestaurantById(restaurantId);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<RestaurantReadDto>(responseResult.Value);

        responseBody.IsCurrentlyOpenToOrder.Should().Be(false);
    }

    #endregion


    #region GetRestaurantsTests

    [Fact]
    public async Task GetRestaurants_Should_Return_A_200Ok()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        var expectedRestaurants = new List<RestaurantReadDto>
        {
            await CreateRestaurant("restaurant1", dateTime),
            await CreateRestaurant("restaurant2", dateTime)
        };

        // Act
        var response = await restaurantsController.GetRestaurants();

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<RestaurantReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(_restaurantAdminInfo.UserId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(true);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(true);
            actualRestaurant.IsPublished.Should().Be(true);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_With_Pagination_Should_Return_A_200Ok_With_Correct_Restaurants()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);
        await CreateRestaurant("restaurant1", dateTime);
        await CreateRestaurant("restaurant2", dateTime);
        await CreateRestaurant("restaurant3", dateTime);
        await CreateRestaurant("restaurant4", dateTime);
        await CreateRestaurant("restaurant5", dateTime);
        await CreateRestaurant("restaurant6", dateTime);
        var expectedRestaurants = new List<RestaurantReadDto>
        {
            await CreateRestaurant("restaurant7", dateTime),
            await CreateRestaurant("restaurant8", dateTime)
        };
        var filter = new RestaurantFilter
        {
            Page = 3,
            Size = 3
        };

        // Act
        var response = await restaurantsController.GetRestaurants(filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<RestaurantReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(2);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(_restaurantAdminInfo.UserId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(true);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(true);
            actualRestaurant.IsPublished.Should().Be(true);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task
        GetRestaurants_With_Filter_IsCurrentlyOpenToOrder_True_Should_Return_A_200Ok_With_Correct_Restaurants()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);

        var restaurant1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurant1.Name = "restaurant1";
        await CreateRestaurant(restaurant1, dateTime);

        var restaurant2 = await CreateRestaurant("restaurant2", dateTime);

        var restaurant3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurant3.Name = "restaurant3";
        await CreateRestaurant(restaurant3, dateTime);

        var restaurant4 = await CreateRestaurant("restaurant4", dateTime);

        var restaurant5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurant5.Name = "restaurant5";
        await CreateRestaurant(restaurant5, dateTime);

        var restaurant6 = await CreateRestaurant("restaurant6", dateTime);

        var restaurant7 = await CreateRestaurant("restaurant7", dateTime);

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant2,
            restaurant4,
            restaurant6,
            restaurant7
        };
        var filter = new RestaurantFilter
        {
            IsCurrentlyOpenToOrder = true
        };

        // Act
        var response = await restaurantsController.GetRestaurants(filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<RestaurantReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(4);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(_restaurantAdminInfo.UserId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(true);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(true);
            actualRestaurant.IsPublished.Should().Be(true);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task
        GetRestaurants_With_Filter_IsCurrentlyOpenToOrder_False_Should_Return_A_200Ok_With_Correct_Restaurants()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);

        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        var restaurant1 = await CreateRestaurant(restaurantCreateDto1, dateTime);

        await CreateRestaurant("restaurant2", dateTime);

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        var restaurant3 = await CreateRestaurant(restaurantCreateDto3, dateTime);

        await CreateRestaurant("restaurant4", dateTime);

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        var restaurant5 = await CreateRestaurant(restaurantCreateDto5, dateTime);

        await CreateRestaurant("restaurant6", dateTime);

        await CreateRestaurant("restaurant7", dateTime);

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant1,
            restaurant3,
            restaurant5
        };
        var filter = new RestaurantFilter
        {
            IsCurrentlyOpenToOrder = false
        };

        // Act
        var response = await restaurantsController.GetRestaurants(filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<RestaurantReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(_restaurantAdminInfo.UserId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.OrderOpeningTimes.Should().BeEmpty();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(false);
            actualRestaurant.IsPublished.Should().Be(false);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_With_Filter_IsPublished_True_Should_Return_A_200Ok_With_Correct_Restaurants()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);

        var restaurant1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurant1.Name = "restaurant1";
        await CreateRestaurant(restaurant1, dateTime);

        var restaurant2 = await CreateRestaurant("restaurant2", dateTime);

        var restaurant3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurant3.Name = "restaurant3";
        await CreateRestaurant(restaurant3, dateTime);

        var restaurant4 = await CreateRestaurant("restaurant4", dateTime);

        var restaurant5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurant5.Name = "restaurant5";
        await CreateRestaurant(restaurant5, dateTime);

        var restaurant6 = await CreateRestaurant("restaurant6", dateTime);

        var restaurant7 = await CreateRestaurant("restaurant7", dateTime);

        var restaurant8 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurant8.Name = "restaurant8";
        await CreateRestaurant(restaurant8, dateTime);

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant2,
            restaurant4,
            restaurant6,
            restaurant7
        };
        var filter = new RestaurantFilter
        {
            IsPublished = true
        };

        // Act
        var response = await restaurantsController.GetRestaurants(filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<RestaurantReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(4);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(_restaurantAdminInfo.UserId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.PlaceOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.PlaceOpeningTimes);
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.PlaceOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenInPlace.Should().Be(true);
            actualRestaurant.OrderOpeningTimes.Should().BeEquivalentTo(expectedRestaurant.OrderOpeningTimes);
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && x.RestaurantId == expectedRestaurant.Id)
                            .Should()
                            .BeTrue();
            actualRestaurant.OrderOpeningTimes
                            .Aggregate(true, (acc, x) => acc && new Regex(GuidUtils.Regex).IsMatch(x.Id))
                            .Should().BeTrue();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(true);
            actualRestaurant.IsPublished.Should().Be(true);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    [Fact]
    public async Task GetRestaurants_With_Filter_IsPublished_False_Should_Return_A_200Ok_With_Correct_Restaurants()
    {
        // Arrange
        var dateTime = DateTime.Parse("2022-03-21T10:00");
        var restaurantsController = InitRestaurantsController(dateTime);

        var restaurantCreateDto1 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto1.Name = "restaurant1";
        var restaurant1 = await CreateRestaurant(restaurantCreateDto1, dateTime);

        await CreateRestaurant("restaurant2", dateTime);

        var restaurantCreateDto3 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto3.Name = "restaurant3";
        var restaurant3 = await CreateRestaurant(restaurantCreateDto3, dateTime);

        await CreateRestaurant("restaurant4", dateTime);

        var restaurantCreateDto5 = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto5.Name = "restaurant5";
        var restaurant5 = await CreateRestaurant(restaurantCreateDto5, dateTime);

        await CreateRestaurant("restaurant6", dateTime);

        await CreateRestaurant("restaurant7", dateTime);

        var expectedRestaurants = new List<RestaurantReadDto>
        {
            restaurant1,
            restaurant3,
            restaurant5
        };

        var filter = new RestaurantFilter
        {
            IsPublished = false
        };

        // Act
        var response = await restaurantsController.GetRestaurants(filter);

        // Assert
        var responseResult = Assert.IsType<OkObjectResult>(response.Result);
        var responseBody = Assert.IsType<List<RestaurantReadDto>>(responseResult.Value);
        responseBody.Count.Should().Be(3);

        for (var i = 0; i < responseBody.Count; i++)
        {
            var actualRestaurant = responseBody[i];
            var expectedRestaurant = expectedRestaurants[i];

            actualRestaurant.Id.Should().MatchRegex(GuidUtils.Regex);
            actualRestaurant.AdminId.Should().Be(_restaurantAdminInfo.UserId);
            actualRestaurant.Email.Should().Be(expectedRestaurant.Email);
            actualRestaurant.PhoneNumber.Should().Be(expectedRestaurant.PhoneNumber);
            actualRestaurant.CreationDateTime.Should().BeCloseTo(dateTime, TimeSpan.FromSeconds(5));
            actualRestaurant.Name.Should().Be(expectedRestaurant.Name);
            actualRestaurant.City.Should().Be(expectedRestaurant.City);
            actualRestaurant.Country.Should().Be(expectedRestaurant.Country);
            actualRestaurant.ZipCode.Should().Be(expectedRestaurant.ZipCode);
            actualRestaurant.StreetName.Should().Be(expectedRestaurant.StreetName);
            actualRestaurant.StreetNumber.Should().Be(expectedRestaurant.StreetNumber);
            actualRestaurant.AddressExtraInformation.Should().Be(expectedRestaurant.AddressExtraInformation);
            actualRestaurant.IsOpen.Should().Be(expectedRestaurant.IsOpen);
            actualRestaurant.IsPublic.Should().Be(expectedRestaurant.IsPublic);
            actualRestaurant.Base64Logo.Should().Be(expectedRestaurant.Base64Logo);
            actualRestaurant.Base64Image.Should().Be(expectedRestaurant.Base64Image);
            actualRestaurant.OrderOpeningTimes.Should().BeEmpty();
            actualRestaurant.IsCurrentlyOpenToOrder.Should().Be(false);
            actualRestaurant.IsPublished.Should().Be(false);
            actualRestaurant.LastUpdateDateTime.Should().BeNull();
            actualRestaurant.EmailConfirmationDateTime.Should().BeNull();
            actualRestaurant.IsEmailConfirmed.Should().Be(false);
        }
    }

    #endregion

    private async Task<RestaurantReadDto> CreateRestaurant(string restaurantName, DateTime dateTime)
    {
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreateDto = RestaurantMocks.SimpleRestaurantCreateDto;
        restaurantCreateDto.Name = restaurantName;

        restaurantCreateDto.ClosingDates = new List<ClosingDateCreateDto>
        {
            new() { ClosingDateTime = dateTime.AddDays(-1) },
            new() { ClosingDateTime = dateTime.AddDays(1) }
        };

        restaurantCreateDto.PlaceOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = dateTime.AddDays(-1).DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = dateTime.DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            }
        };

        restaurantCreateDto.OrderOpeningTimes = new List<OpeningTimeCreateDto>
        {
            new()
            {
                DayOfWeek = dateTime.AddDays(-1).DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            },
            new()
            {
                DayOfWeek = dateTime.DayOfWeek,
                OffsetInMinutes = dateTime.Hour * 60 + dateTime.Minute,
                DurationInMinutes = 2 * 60
            }
        };

        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        return Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
    }

    private async Task<RestaurantReadDto> CreateRestaurant(RestaurantCreateDto restaurantCreateDto, DateTime dateTime)
    {
        var restaurantsController = InitRestaurantsController(dateTime);
        var restaurantCreationResponse = await restaurantsController.CreateRestaurant(restaurantCreateDto);
        var restaurantCreationResponseResult = Assert.IsType<CreatedResult>(restaurantCreationResponse.Result);
        return Assert.IsType<RestaurantReadDto>(restaurantCreationResponseResult.Value);
    }
}
