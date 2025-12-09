using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Auth.Commands.Register;
using MediaTR.SharedKernel.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Auth;

/// <summary>
/// User registration endpoint
/// </summary>
internal sealed class Register : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register", async (
            [FromBody] RegisterRequest request,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Create command
            RegisterCommand command = new(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.UserName,
                request.PhoneNumber
            );

            // Execute command
            RegisterResponse result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return Results.Ok(result);
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account with email and password")
        .WithOpenApi()
        .Produces<RegisterResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}

/// <summary>
/// Register request DTO for API
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string UserName,
    string? PhoneNumber = null);
