using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
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
            [FromBody] RefreshTokenRequest? request,
            CancellationToken cancellationToken) =>
        {
            // Detect platform and get IP address
            bool isMobile = httpContext.IsMobileClient();
            string ipAddress = httpContext.GetClientIpAddress();

            // Platform-aware token reading
            string? refreshToken = null;
            if (isMobile)
            {
                // Mobile: Read from request body
                refreshToken = request?.RefreshToken;
            }
            else
            {
                // Web: Read from HttpOnly cookie
                httpContext.Request.Cookies.TryGetValue("refreshToken", out refreshToken);
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.Unauthorized();
            }

            // Create command
            RefreshTokenCommand command = new(refreshToken);

            // Execute command
            LoginResponse result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            // Platform-specific response
            if (isMobile)
            {
                // Mobile: Return RefreshToken in JSON response
                var mobileResponse = new LoginMobileApiResponse(
                    result.UserId,
                    result.UserName,
                    result.Email,
                    result.AccessToken,
                    result.RefreshToken,
                    result.AccessTokenExpiresAt,
                    result.RefreshTokenExpiresAt
                );

                return Results.Ok(mobileResponse);
            }
            else
            {
                // Web: Set new RefreshToken as HttpOnly cookie (token rotation)
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
                var webResponse = new LoginApiResponse(
                    result.UserId,
                    result.UserName,
                    result.Email,
                    result.AccessToken,
                    result.AccessTokenExpiresAt
                );

                return Results.Ok(webResponse);
            }
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh access token")
        .WithDescription("Generate new access and refresh tokens. Web clients use HttpOnly cookie, mobile clients send token in request body.")
        .WithOpenApi()
        .Produces<LoginApiResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}

/// <summary>
/// Refresh token request DTO for mobile clients
/// </summary>
public record RefreshTokenRequest(string RefreshToken);
