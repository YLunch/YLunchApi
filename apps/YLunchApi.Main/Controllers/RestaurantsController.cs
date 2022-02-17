using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Services;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Main.Controllers;

[ApiController]
[Route("[Controller]")]
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
            var restaurantReadDto = await _restaurantService.Create(restaurantCreateDto, CurrentUserId!);
            return Created("", restaurantReadDto);
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict("Restaurant already exists");
        }
    }

    [HttpGet("{restaurantId}")]
    public async Task<ActionResult<RestaurantReadDto>> GetRestaurantById(string restaurantId)
    {
        try
        {
            var restaurantReadDto = await _restaurantService.GetById(restaurantId);
            return Ok(restaurantReadDto);
        }
        catch (EntityNotFoundException)
        {
            return NotFound($"Restaurant {restaurantId} not found");
        }
    }
}
