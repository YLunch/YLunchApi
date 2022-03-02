using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YLunchApi.Domain.Exceptions;
using YLunchApi.Domain.ProductsAggregate.Dto;
using YLunchApi.Domain.UserAggregate.Models;

namespace YLunchApi.Main.Controllers;

[ApiController]
[Route("[Controller]")]
public class ProductsController: ApplicationControllerBase
{
    private readonly IProductService _ProductService;
    
    public ProductsController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _ProductService = productService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.RestaurantAdmin)]
    public async Task<ActionResult<ProductReadDto>> CreatProduct([FromBody] ProductCreateDto productCreateDto)
    {
        try
        {
           var productReadDto = await _ProductService.Create(productCreateDto, CurrentUserId!);
            return Created("", productReadDto);
        }
        catch (EntityAlreadyExistsException)
        {
            return Conflict("Product already exists");
        }
    }
    
    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductReadDto>> GetProductById(string productId)
    {
        try
        {
           var productReadDto = await _ProductService.GetById(productId);
            return Ok( productReadDto);
        }
        catch (EntityNotFoundException)
        {
            return NotFound($"Product {productId} not found");
        }
    }
}