using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using YLunch.Api.Core;
using YLunch.Application.Exceptions;
using YLunch.Domain.ModelsAggregate.UserAggregate;
using YLunch.Domain.ModelsAggregate.UserAggregate.Roles;
using YLunch.Domain.Services.Database.Repositories;
using YLunch.Domain.Services.RestaurantServices;

namespace YLunch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : CustomControllerBase
    {
        private readonly IRestaurantService _restaurantService;

        public CustomerController(
            UserManager<User> userManager,
            IUserRepository userRepository,
            IConfiguration configuration,
            IRestaurantService restaurantService
        ) : base(userManager, userRepository, configuration)
        {
            _restaurantService = restaurantService;
        }

        [HttpGet("get-all-restaurants")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRestaurants()
        {
            try
            {
                return Ok(await _restaurantService.GetAllForCustomer());
            }
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    e
                );
            }
        }
    }
}
