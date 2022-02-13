using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Services;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Main.Controllers;

public class RestaurantsController : ApplicationControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantsController(IHttpContextAccessor httpContextAccessor,
                                 IRestaurantService restaurantService) : base(httpContextAccessor)
    {
        _restaurantService = restaurantService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public async Task<ActionResult<RestaurantReadDto>> CreateRestaurant(
        [FromBody] RestaurantCreateDto restaurantCreateDto)
    {
        try
        {
            RestaurantReadDto restaurantReadDto = await _restaurantService.Create(restaurantCreateDto, CurrentUserId!);
            return Created("", restaurantReadDto);
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict("Restaurant already exists");
        }
    }
}
