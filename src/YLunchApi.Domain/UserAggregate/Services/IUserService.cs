using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.UserAggregate.Services;

public interface IUserService
{
    Task<UserReadDto?> CreateUser(UserCreateDto userCreateDto, string role);

    Task<AuthenticatedUser> GetAuthenticatedUser(LoginRequestDto loginRequestDto);

    Task<UserReadDto> GetUserById(string id);
}
