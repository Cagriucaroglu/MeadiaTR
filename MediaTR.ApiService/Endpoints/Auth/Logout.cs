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
            HttpContext httpContext,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Read RefreshToken from HttpOnly cookie
            if (httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)
                && !string.IsNullOrEmpty(refreshToken))
            {
                // Get IP address from HttpContext
                string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // Create command to revoke refresh token in database
                LogoutCommand command = new(refreshToken);

                // Execute command
                await sender.Send(command, cancellationToken).ConfigureAwait(false);
            }

            // Delete the HttpOnly cookie (regardless of whether token was valid)
            httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return Results.Ok(new { Success = true, Message = "Logged out successfully" });
        })
        .WithName("Logout")
        .WithSummary("User logout")
        .WithDescription("Revoke refresh token in database and delete HttpOnly cookie")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .AllowAnonymous(); // Anyone can logout (no auth required)
    }
}
