using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;

namespace YLunchApi.Application.UserAggregate;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserReadDto?> CreateUser(UserCreateDto userCreateDto, string role)
    {
        var user = new User(userCreateDto);

        var userDb = await _userRepository.GetUserByEmail(userCreateDto.Email);
        if (userDb != null) throw new EntityAlreadyExistsException();

        await _userRepository.CreateUser(user, userCreateDto.Password, role);

        userDb = await _userRepository.GetUserByEmail(user.Email);
        if (userDb == null) throw new EntityNotFoundException();

        var roles = await _userRepository.GetUserRoles(userDb);

        return new UserReadDto(userDb, roles);
    }

    public async Task<AuthenticatedUser> GetAuthenticatedUser(LoginRequestDto loginRequestDto)
    {
        var user = await _userRepository.GetUserByEmailAndPassword(loginRequestDto.Email.ToLower(),
            loginRequestDto.Password);
        if (user == null) throw new EntityNotFoundException($"{loginRequestDto.Email} not found.");
        var roles = await _userRepository.GetUserRoles(user);
        return new AuthenticatedUser(user, roles);
    }

    public async Task<UserReadDto> GetUserById(string id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null) throw new EntityNotFoundException($"User {id} not found.");

        var roles = await _userRepository.GetUserRoles(user);
        return new UserReadDto(user, roles);
    }
}
