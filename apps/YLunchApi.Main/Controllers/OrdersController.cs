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
            var orderReadDto = await _orderService.CreateOrder(CurrentUserId!, restaurantId, orderCreateDto);
            return Created("", orderReadDto);
        }
        catch (EntityNotFoundException exception)
        {
            return exception.Message switch
            {
                { } m when m.Contains("Product") => NotFound(new ErrorDto(HttpStatusCode.NotFound, exception.Message)),
                _ => NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {restaurantId} not found."))
            };
        }
        catch (ReservedForDateTimeOutOfOpenToOrderOpeningTimesException)
        {
            return BadRequest(new ErrorDto(HttpStatusCode.BadRequest, "ReservedForDateTime must be set when the restaurant is open for orders."));
        }
    }

    [HttpGet("orders/{orderId}")]
    [Authorize]
    public async Task<ActionResult<OrderReadDto>> GetOrderById(string orderId)
    {
        try
        {
            return CurrentUserRoles.Contains(Roles.RestaurantAdmin) ?
                Ok(await _orderService.GetOrderByIdForRestaurantAdmin(CurrentUserId!, orderId)) :
                Ok(await _orderService.GetOrderByIdForCustomer(CurrentUserId!, orderId));
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Order: {orderId} not found."));
        }
    }

    [HttpGet("restaurants/{restaurantId}/orders")]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public async Task<ActionResult<ICollection<OrderReadDto>>> GetOrdersByRestaurantId([FromRoute] string restaurantId, [FromQuery] OrderFilter? orderFilter = null)
    {
        try
        {
            var restaurant = await _restaurantService.GetRestaurantById(restaurantId);
            var filter = orderFilter ?? new OrderFilter();
            filter.RestaurantId = restaurant.Id;
            var ordersReadDto = await _orderService.GetOrders(filter);
            return Ok(ordersReadDto);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {restaurantId} not found."));
        }
    }

    [HttpPost("restaurants/{restaurantId}/orders/statuses")]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public async Task<ActionResult<ICollection<OrderReadDto>>> AddStatusToOrders([FromRoute] string restaurantId, [FromBody] AddStatusToOrdersDto addStatusToOrdersDto)
    {
        try
        {
            await _restaurantService.GetRestaurantById(restaurantId);
            var ordersReadDto = await _orderService.AddStatusToOrders(restaurantId, addStatusToOrdersDto);
            return Ok(ordersReadDto);
        }
        catch (EntityNotFoundException exception)
        {
            return exception.Message switch
            {
                { } m when m.Contains("Orders") => NotFound(new ErrorDto(HttpStatusCode.NotFound, exception.Message)),
                _ => NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {restaurantId} not found."))
            };
        }
    }
}
