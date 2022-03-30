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
public class ProductsController : ApplicationControllerBase
{
    private readonly IProductService _productService;
    private readonly IRestaurantService _restaurantService;

    public ProductsController(IHttpContextAccessor httpContextAccessor, IProductService productService, IRestaurantService restaurantService) : base(
        httpContextAccessor)
    {
        _productService = productService;
        _restaurantService = restaurantService;
    }

    [HttpPost("Restaurants/{restaurantId}/products")]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public async Task<ActionResult<ProductReadDto>> CreateProduct([FromRoute] string restaurantId, [FromBody] ProductCreateDto productCreateDto)
    {
        try
        {
            var restaurant = await _restaurantService.GetById(restaurantId);
            var productReadDto = await _productService.Create(productCreateDto, restaurant.Id);
            return Created("", productReadDto);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {restaurantId} not found."));
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict(new ErrorDto(HttpStatusCode.Conflict, $"Product: {productCreateDto.Name} already exists."));
        }
    }

    [HttpGet("products/{productId}")]
    public async Task<ActionResult<ProductReadDto>> GetProductById([FromRoute] string productId)
    {
        try
        {
            var productReadDto = await _productService.GetById(productId);
            return Ok(productReadDto);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Product {productId} not found."));
        }
    }

    [HttpGet("restaurants/{restaurantId}/products")]
    public async Task<ActionResult<ICollection<ProductReadDto>>> GetProductsByRestaurantId([FromRoute] string restaurantId, [FromQuery] ProductFilter? productFilter = null)
    {
        try
        {
            var restaurant = await _restaurantService.GetById(restaurantId);
            var filter = productFilter ?? new ProductFilter();
            filter.RestaurantId = restaurant.Id;
            var productsReadDto = await _productService.GetProducts(filter);
            return Ok(productsReadDto);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Restaurant: {restaurantId} not found."));
        }
    }
}
