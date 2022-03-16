using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<ProductReadDto>> CreateProduct([FromBody] ProductCreateDto productCreateDto,
        [FromRoute] string restaurantId)
    {
        try
        {
            var productReadDto = await _productService.Create(productCreateDto, restaurantId);
            return Created("", productReadDto);
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict("Product already exists");
        }
    }
}