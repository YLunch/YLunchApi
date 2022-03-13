using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Main.Controllers;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Configuration;

namespace YLunchApi.UnitTests.Controllers;

public class UsersControllerTest : UnitTestFixture
{
    private readonly UsersController _usersController;

    public UsersControllerTest(UnitTestFixtureBase fixtureBase) : base(fixtureBase)
    {
        Fixture.InitFixture();
        _usersController = Fixture.GetImplementationFromService<UsersController>();
    }

    [Fact]
    public async Task Should_Create_A_Customer_User()
    {
        // Act
        var response = await _usersController.Create(UserMocks.CustomerCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<UserReadDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(UserMocks.CustomerUserReadDto(responseBody.Id));
    }

    [Fact]
    public async Task Should_Create_A_RestaurantAdmin_User()
    {
        // Act
        var response = await _usersController.Create(UserMocks.RestaurantAdminCreateDto);

        // Assert
        var responseResult = Assert.IsType<CreatedResult>(response.Result);
        var responseBody = Assert.IsType<UserReadDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(UserMocks.RestaurantAdminUserReadDto(responseBody.Id));
    }

    [Fact]
    public async Task Should_Return_A_409ConflictError()
    {
        // Act
        _ = await _usersController.Create(UserMocks.RestaurantAdminCreateDto);
        var response = await _usersController.Create(UserMocks.RestaurantAdminCreateDto);

        // Assert
        var responseResult = Assert.IsType<ConflictObjectResult>(response.Result);
        var responseBody = Assert.IsType<MessageDto>(responseResult.Value);

        responseBody.Should().BeEquivalentTo(new MessageDto("User already exists"));
    }
}
