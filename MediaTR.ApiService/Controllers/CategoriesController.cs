using MediatR;
using MediaTR.Application.Features.Categories.Commands;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        // TODO: Implement GetCategoriesQuery
        return Ok("Get all categories");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(string id)
    {
        // TODO: Implement GetCategoryQuery
        return Ok($"Get category {id}");
    }

    [HttpGet("root")]
    public async Task<IActionResult> GetRootCategories()
    {
        // TODO: Implement GetRootCategoriesQuery
        return Ok("Get root categories");
    }

    [HttpGet("{id}/children")]
    public async Task<IActionResult> GetChildCategories(string id)
    {
        // TODO: Implement GetChildCategoriesQuery
        return Ok($"Get child categories of {id}");
    }
}