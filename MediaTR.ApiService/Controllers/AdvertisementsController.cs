using MediatR;
using MediaTR.Application.Features.Advertisements.Commands;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdvertisementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdvertisement([FromBody] CreateAdvertisementCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveAdvertisement(Guid id)
    {
        ApproveAdvertisementCommand command = new(id);
        await _mediator.Send(command);
        return Ok($"Advertisement {id} approved");
    }

    [HttpGet]
    public async Task<IActionResult> GetAdvertisements()
    {
        // TODO: Implement GetAdvertisementsQuery
        return Ok("Get all advertisements");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdvertisement(string id)
    {
        // TODO: Implement GetAdvertisementQuery
        return Ok($"Get advertisement {id}");
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingAdvertisements()
    {
        // TODO: Implement GetPendingAdvertisementsQuery
        return Ok("Get pending advertisements");
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedAdvertisements()
    {
        // TODO: Implement GetFeaturedAdvertisementsQuery
        return Ok("Get featured advertisements");
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserAdvertisements(Guid userId)
    {
        // TODO: Implement GetUserAdvertisementsQuery
        return Ok($"Get advertisements for user {userId}");
    }
}