using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Auth.Commands.Login;
using MediaTR.Application.Features.Auth.Commands.RefreshToken;
using MediaTR.SharedKernel.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Auth;

/// <summary>
/// Refresh token endpoint
/// </summary>
internal sealed class RefreshToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh", async (
            [FromBody] RefreshTokenRequest request,
            HttpContext httpContext,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Get IP address from HttpContext
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Create command
            RefreshTokenCommand command = new(request.RefreshToken);

            // Execute command
            LoginResponse result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh access token")
        .WithDescription("Generate new access and refresh tokens using a valid refresh token")
        .WithOpenApi()
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}

/// <summary>
/// Refresh token request DTO for API
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken);
