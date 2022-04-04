using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.CommonAggregate.Dto;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Services;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Main.Controllers;

[ApiController]
[Route("")]
public class OrdersController : ApplicationControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IRestaurantService _restaurantService;

    public OrdersController(IHttpContextAccessor httpContextAccessor, IOrderService orderService, IRestaurantService restaurantService) : base(
        httpContextAccessor)
    {
        _orderService = orderService;
        _restaurantService = restaurantService;
    }

    [HttpPost("Restaurants/{restaurantId}/orders")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<OrderReadDto>> CreateOrder([FromRoute] string restaurantId, [FromBody] OrderCreateDto orderCreateDto)
    {
        try
        {
            var restaurant = await _restaurantService.GetById(restaurantId);
            var orderReadDto = await _orderService.Create(CurrentUserId!, restaurant.Id, orderCreateDto);
            return Created("", orderReadDto);
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {restaurantId} not found."));
        }
    }
}
