using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.Domain.UserAggregate.Dto;

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

    public async Task<User?> GetAuthenticatedUser(LoginRequestDto loginRequestDto)
    {
        return await _userRepository.GetByEmailAndPassword(loginRequestDto.Email.ToLower(), loginRequestDto.Password);
    }
}