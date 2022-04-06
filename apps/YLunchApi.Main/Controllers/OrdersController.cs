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

    public OrdersController(IHttpContextAccessor httpContextAccessor, IOrderService orderService) : base(
        httpContextAccessor)
    {
        _orderService = orderService;
    }

    [HttpPost("Restaurants/{restaurantId}/orders")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<OrderReadDto>> CreateOrder([FromRoute] string restaurantId, [FromBody] OrderCreateDto orderCreateDto)
    {
        try
        {
            var orderReadDto = await _orderService.Create(CurrentUserId!, restaurantId, orderCreateDto);
            return Created("", orderReadDto);
        }
        catch (EntityNotFoundException)
        {
            // todo filter exception for restaurant or product not found
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {restaurantId} not found."));
        }
        catch (ReservedForDateTimeOutOfOpenToOrderOpeningTimesException)
        {
            return BadRequest(new ErrorDto(HttpStatusCode.BadRequest, "ReservedForDateTime must be set when the restaurant is open for orders."));
        }
    }
}
