using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Auth.Commands.Login;
using MediaTR.SharedKernel.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Auth;

/// <summary>
/// User login endpoint
/// </summary>
internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (
            [FromBody] LoginRequest request,
            HttpContext httpContext,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Get IP address from HttpContext
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Create command
            LoginCommand command = new(
                request.EmailOrUsername,
                request.Password
            );

            // Execute command
            LoginResponse result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("User login")
        .WithDescription("Authenticate user and return JWT access token and refresh token")
        .WithOpenApi()
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
        // .RequireRateLimiting("login-limiter"); // Phase D'de eklenecek
    }
}

/// <summary>
/// Login request DTO for API
/// </summary>
public record LoginRequest(
    string EmailOrUsername,
    string Password);
