using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Domain.UserAggregate.Services;

public interface IUserService
{
    Task<UserReadDto?> Create(UserCreateDto userCreateDto, string role);

    Task<AuthenticatedUser> GetAuthenticatedUser(LoginRequestDto loginRequestDto);

    Task<UserReadDto> GetById(string id);
}
