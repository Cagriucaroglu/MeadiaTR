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

            // Set RefreshToken as HttpOnly cookie (XSS protection)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,        // JavaScript cannot access (XSS protection)
                Secure = true,          // Only HTTPS (set false for development if needed)
                SameSite = SameSiteMode.Strict, // CSRF protection
                Expires = result.RefreshTokenExpiresAt,
                Path = "/",
                IsEssential = true      // GDPR exemption for authentication
            };
            httpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            // Return only AccessToken in response body (RefreshToken is in cookie)
            var apiResponse = new LoginApiResponse(
                result.UserId,
                result.UserName,
                result.Email,
                result.AccessToken,
                result.AccessTokenExpiresAt
            );

            return Results.Ok(apiResponse);
        })
        .WithName("Login")
        .WithSummary("User login")
        .WithDescription("Authenticate user and return JWT access token. RefreshToken is set as HttpOnly cookie.")
        .WithOpenApi()
        .Produces<LoginApiResponse>(StatusCodes.Status200OK)
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

/// <summary>
/// Login response DTO for API (RefreshToken excluded, sent as HttpOnly cookie)
/// </summary>
public record LoginApiResponse(
    Guid UserId,
    string UserName,
    string Email,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt);
