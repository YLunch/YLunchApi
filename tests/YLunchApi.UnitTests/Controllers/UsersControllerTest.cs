using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using YLunchApi.Application.UserAggregate;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.Main.Controllers;
using YLunchApi.UnitTests.Application.UserAggregate;
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Controllers;

public class UsersControllerTest
{
    private readonly UsersController _usersController;

    public UsersControllerTest()
    {
        var context = ContextBuilder.BuildContext();

        var roleManagerMock = ManagerMocker.GetRoleManagerMock(context);
        var userManagerMock = ManagerMocker.GetUserManagerMock(context);

        var userRepository = new UserRepository(context, userManagerMock.Object, roleManagerMock.Object);
        var userService = new UserService(userRepository);

        _usersController = new UsersController(userService, HttpContextAccessorMocker.GetWithoutAuthorization());
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
        var responseBody = Assert.IsType<string>(responseResult.Value);

        responseBody.Should().Be("User already exists");
    }
}
