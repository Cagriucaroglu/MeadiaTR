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
            HttpContext httpContext,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Read RefreshToken from HttpOnly cookie
            if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)
                || string.IsNullOrEmpty(refreshToken))
            {
                return Results.Unauthorized();
            }

            // Get IP address from HttpContext
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Create command
            RefreshTokenCommand command = new(refreshToken);

            // Execute command
            LoginResponse result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            // Set new RefreshToken as HttpOnly cookie (token rotation)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAt,
                Path = "/",
                IsEssential = true
            };
            httpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            // Return only AccessToken in response body
            var apiResponse = new LoginApiResponse(
                result.UserId,
                result.UserName,
                result.Email,
                result.AccessToken,
                result.AccessTokenExpiresAt
            );

            return Results.Ok(apiResponse);
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh access token")
        .WithDescription("Generate new access and refresh tokens using HttpOnly cookie. Implements token rotation.")
        .WithOpenApi()
        .Produces<LoginApiResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
