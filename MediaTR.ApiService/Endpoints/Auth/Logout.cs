using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Auth.Commands.Logout;
using MediaTR.SharedKernel.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Auth;

/// <summary>
/// User logout endpoint
/// </summary>
internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/logout", async (
            [FromBody] LogoutRequest request,
            HttpContext httpContext,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Get IP address from HttpContext
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Create command
            LogoutCommand command = new(request.RefreshToken);

            // Execute command
            bool result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return Results.Ok(new { Success = result, Message = "Logged out successfully" });
        })
        .WithName("Logout")
        .WithSummary("User logout")
        .WithDescription("Revoke refresh token and invalidate user session")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .AllowAnonymous(); // Anyone can logout with valid refresh token
    }
}

/// <summary>
/// Logout request DTO for API
/// </summary>
public record LogoutRequest(
    string RefreshToken);
