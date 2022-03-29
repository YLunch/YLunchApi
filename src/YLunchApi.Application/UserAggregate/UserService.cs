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

    public async Task<UserReadDto?> Create(UserCreateDto userCreateDto, string role)
    {
        var user = new User(userCreateDto);

        var userDb = await _userRepository.GetByEmail(userCreateDto.Email);
        if (userDb != null) throw new EntityAlreadyExistsException();

        await _userRepository.Create(user, userCreateDto.Password, role);

        userDb = await _userRepository.GetByEmail(user.Email);
        if (userDb == null) throw new EntityNotFoundException();

        var roles = await _userRepository.GetUserRoles(userDb);

        return new UserReadDto(userDb, roles);
    }

    public async Task<AuthenticatedUser> GetAuthenticatedUser(LoginRequestDto loginRequestDto)
    {
        var user = await _userRepository.GetByEmailAndPassword(loginRequestDto.Email.ToLower(),
            loginRequestDto.Password);
        if (user == null) throw new EntityNotFoundException($"{loginRequestDto.Email} not found.");
        var roles = await _userRepository.GetUserRoles(user);
        return new AuthenticatedUser(user, roles);
    }

    public async Task<UserReadDto> GetById(string id)
    {
        var user = await _userRepository.GetById(id);
        if (user == null) throw new EntityNotFoundException($"User {id} not found.");

        var roles = await _userRepository.GetUserRoles(user);
        return new UserReadDto(user, roles);
    }
}
