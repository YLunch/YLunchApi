using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;
using YLunchApi.Infrastructure.Database;
using YLunchApi.TestsShared.Mocks;
using YLunchApi.UnitTests.Core.Configuration;
using YLunchApi.UnitTests.Core.Mocks;

namespace YLunchApi.UnitTests.Application.UserAggregate;

public class UserServiceTest : UnitTestFixture
{
    public UserServiceTest(UnitTestFixtureBase fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Should_Create_RestaurantAdmin()
    {
        // Arrange
        var userCreateDto = UserMocks.RestaurantAdminCreateDto;
        Fixture.InitFixture();
        var userService = Fixture.GetImplementationFromService<IUserService>();

        // Act
        var actual = await userService.Create(userCreateDto, Roles.RestaurantAdmin);

        // Assert
        actual = Assert.IsType<UserReadDto>(actual);
        var expected = UserMocks.RestaurantAdminUserReadDto(actual.Id);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_Create_Customer()
    {
        // Arrange
        var userCreateDto = UserMocks.CustomerCreateDto;
        Fixture.InitFixture();
        var userService = Fixture.GetImplementationFromService<IUserService>();

        // Act
        var actual = await userService.Create(userCreateDto, Roles.Customer);

        // Assert
        actual = Assert.IsType<UserReadDto>(actual);
        var expected = UserMocks.CustomerUserReadDto(actual.Id);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Create_Should_Throw_EntityAlreadyExistsException()
    {
        // Arrange
        var userCreateDto = UserMocks.CustomerCreateDto;
        Fixture.InitFixture();
        var userService = Fixture.GetImplementationFromService<IUserService>();
        await userService.Create(userCreateDto, Roles.Customer);

        // Act
        async Task Act() => await userService.Create(userCreateDto, Roles.Customer);

        // Assert
        await Assert.ThrowsAsync<EntityAlreadyExistsException>(Act);
    }

    [Fact]
    public async Task Create_Should_Throw_EntityNotFoundException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(x => x.GetByEmail(It.IsAny<string>()))
                          .ReturnsAsync(() => null);
        Fixture.InitFixture(configuration => configuration.UserRepository = userRepositoryMock.Object);
        var userService = Fixture.GetImplementationFromService<IUserService>();
        var userCreateDto = UserMocks.CustomerCreateDto;

        // Act
        async Task Act() => await userService.Create(userCreateDto, Roles.Customer);

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(Act);
    }

    [Fact]
    public async Task Create_Should_Throw_EntityNotFoundException_When_Role_Is_Unknown()
    {
        // Arrange
        var userCreateDto = UserMocks.CustomerCreateDto;
        Fixture.InitFixture();
        var userService = Fixture.GetImplementationFromService<IUserService>();

        // Act
        async Task Act() => await userService.Create(userCreateDto, "UnknownRole");

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(Act);
    }

    [Fact]
    public async Task Create_Should_Throw_UserRegistrationException()
    {
        // Arrange
        Fixture.InitFixture();
        var context = Fixture.GetImplementationFromService<ApplicationDbContext>();
        var userManagerMock = new Mock<UserManagerMock>(context);
        userManagerMock.Setup(x => x.CreateAsync(
                           It.IsAny<User>(),
                           It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Failed());
        Fixture.InitFixture(configuration => configuration.UserManager = userManagerMock.Object);
        var userService = Fixture.GetImplementationFromService<IUserService>();

        var userCreateDto = UserMocks.CustomerCreateDto;

        // Act
        async Task Act() => await userService.Create(userCreateDto, Roles.Customer);

        // Assert
        await Assert.ThrowsAsync<UserRegistrationException>(Act);
    }
}
