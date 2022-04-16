using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Filters;
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
            var restaurantReadDto = await _restaurantService.CreateRestaurant(restaurantCreateDto, CurrentUserId!);
            return Created("", restaurantReadDto);
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict(new ErrorDto(HttpStatusCode.Conflict, "Restaurant already exists."));
        }
    }

    [HttpGet("{restaurantId}")]
    public async Task<ActionResult<RestaurantReadDto>> GetRestaurantById(string restaurantId)
    {
        try
        {
            var restaurantReadDto = await _restaurantService.GetRestaurantById(restaurantId);
            return Ok(restaurantReadDto);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant {restaurantId} not found."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<RestaurantReadDto>>> GetRestaurants([FromQuery] RestaurantFilter? restaurantFilter = null)
    {
        var filter = restaurantFilter ?? new RestaurantFilter();
        var restaurantReadDto = await _restaurantService.GetRestaurants(filter);
        return Ok(restaurantReadDto);
    }
}
