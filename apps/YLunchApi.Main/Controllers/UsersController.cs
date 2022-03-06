using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.UserAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;
using YLunchApi.Domain.UserAggregate.Services;

namespace YLunchApi.Main.Controllers;

[ApiController]
[Route("")]
public class UsersController : ApplicationControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService, IHttpContextAccessor httpContextAccessor) : base(
        httpContextAccessor)
    {
        _userService = userService;
    }

    [HttpPost("restaurant-admins")]
    public async Task<ActionResult<UserReadDto>> Create([FromBody] RestaurantAdminCreateDto restaurantAdminCreateDto)
    {
        return await Create(restaurantAdminCreateDto, Roles.RestaurantAdmin);
    }

    [HttpPost("customers")]
    public async Task<ActionResult<UserReadDto>> Create([FromBody] CustomerCreateDto customerCreateDto)
    {
        return await Create(customerCreateDto, Roles.Customer);
    }

    private async Task<ActionResult<UserReadDto>> Create(UserCreateDto userCreateDto, string role)
    {
        try
        {
            var userReadDto = await _userService.Create(userCreateDto, role);
            return Created("", userReadDto);
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict(new MessageDto("User already exists"));
        }
    }
}
