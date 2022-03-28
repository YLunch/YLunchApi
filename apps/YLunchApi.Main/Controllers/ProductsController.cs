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
public class ProductsController : ApplicationControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IHttpContextAccessor httpContextAccessor, IProductService productService) : base(
        httpContextAccessor)
    {
        _productService = productService;
    }

    [HttpPost("Restaurants/{restaurantId}/products")]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public async Task<ActionResult<ProductReadDto>> CreateProduct([FromRoute] string restaurantId, [FromBody] ProductCreateDto productCreateDto)
    {
        try
        {
            var productReadDto = await _productService.Create(productCreateDto, restaurantId);
            return Created("", productReadDto);
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict(new ErrorDto(HttpStatusCode.Conflict, $"Product: {productCreateDto.Name} already exists"));
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
            return NotFound(new ErrorDto(HttpStatusCode.NotFound, $"Product {productId} not found"));
        }
    }
}
