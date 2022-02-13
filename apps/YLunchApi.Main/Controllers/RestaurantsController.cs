using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Main.Controllers;

public class RestaurantsController : ApplicationControllerBase
{
    public RestaurantsController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    [HttpPost]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public async Task<ActionResult<RestaurantReadDto>> CreateRestaurant(
        [FromBody] RestaurantCreateDto restaurantCreateDto)
    {
        return await Task.FromResult(Created("", "works"));
    }
}
