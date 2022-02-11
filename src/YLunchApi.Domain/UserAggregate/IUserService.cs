using YLunchApi.Domain.UserAggregate.Dto;

namespace YLunchApi.Domain.UserAggregate;

public interface IUserService
{
    Task<UserReadDto?> Create(UserCreateDto userCreateDto, string role);

    Task<AuthenticatedUser> GetAuthenticatedUser(LoginRequestDto loginRequestDto);

    Task<UserReadDto> GetById(string id);
}
