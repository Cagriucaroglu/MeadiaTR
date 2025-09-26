using MediatR;
using MediaTR.Application.Features.Products.Commands;
using MediaTR.Application.Features.Products.Queries;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        GetProductQuery query = new (id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // TODO: Implement GetProductsQuery
        return Ok("Get all products endpoint");
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
    {
        // TODO: Implement GetProductsByCategoryQuery
        return Ok($"Get products by category {categoryId}");
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedProducts()
    {
        // TODO: Implement GetFeaturedProductsQuery
        return Ok("Get featured products");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
    {
        // TODO: Implement SearchProductsQuery
        return Ok($"Search products for: {searchTerm}");
    }
}