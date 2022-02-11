using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using YLunchApi.Application.UserAggregate;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Infrastructure.Database;
using YLunchApi.Infrastructure.Database.Repositories;
using YLunchApi.UnitTests.Core;
using YLunchApi.UnitTests.Core.Mockers;

namespace YLunchApi.UnitTests.Application.UserAggregate;

public class UserServiceTest
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private IUserService _userService;

    public UserServiceTest()
    {
        _context = ContextBuilder.BuildContext();

        _roleManagerMock = ManagerMocker.GetRoleManagerMock(_context);
        _userManagerMock = ManagerMocker.GetUserManagerMock(_context);

        var userRepository = new UserRepository(_context, _userManagerMock.Object, _roleManagerMock.Object);
        _userService = new UserService(userRepository);
    }

    [Fact]
    public async Task Should_Create_RestaurantAdmin()
    {
        // Arrange
        var userCreateDto = UserMocks.RestaurantAdminCreateDto;

        // Act
        var actual = await _userService.Create(userCreateDto, Roles.RestaurantAdmin);

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

        // Act
        var actual = await _userService.Create(userCreateDto, Roles.Customer);

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
        await _userService.Create(userCreateDto, Roles.Customer);

        // Act
        async Task Act() => await _userService.Create(userCreateDto, Roles.Customer);

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
        _userService = new UserService(userRepositoryMock.Object);
        var userCreateDto = UserMocks.CustomerCreateDto;

        // Act
        async Task Act() => await _userService.Create(userCreateDto, Roles.Customer);

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(Act);
    }

    [Fact]
    public async Task Create_Should_Throw_EntityNotFoundException_When_Role_Is_Unknown()
    {
        // Arrange
        var userCreateDto = UserMocks.CustomerCreateDto;

        // Act
        async Task Act() => await _userService.Create(userCreateDto, "UnknownRole");

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(Act);
    }

    [Fact]
    public async Task Create_Should_Throw_UserRegistrationException()
    {
        // Arrange
        _userManagerMock.Setup(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        var userRepository = new UserRepository(_context, _userManagerMock.Object, _roleManagerMock.Object);

        _userService = new UserService(userRepository);

        var userCreateDto = UserMocks.CustomerCreateDto;

        // Act
        async Task Act() => await _userService.Create(userCreateDto, Roles.Customer);

        // Assert
        await Assert.ThrowsAsync<UserRegistrationException>(Act);
    }
}
