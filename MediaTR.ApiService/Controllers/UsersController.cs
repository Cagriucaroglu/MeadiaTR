using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser()
    {
        // TODO: Implement RegisterUserCommand
        return Ok("User registration endpoint");
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser()
    {
        // TODO: Implement LoginUserCommand
        return Ok("User login endpoint");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        // TODO: Implement GetUserQuery
        return Ok($"Get user {id}");
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // TODO: Implement GetUsersQuery
        return Ok("Get all users");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id)
    {
        // TODO: Implement UpdateUserCommand
        return Ok($"Update user {id}");
    }
}