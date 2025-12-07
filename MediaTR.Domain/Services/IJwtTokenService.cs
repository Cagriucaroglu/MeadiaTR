using MediaTR.Domain.Entities;

namespace MediaTR.Domain.Services;

/// <summary>
/// Service for generating, validating, and managing JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates access and refresh tokens for a user.
    /// </summary>
    /// <param name="user">User to generate tokens for</param>
    /// <param name="ipAddress">IP address of the requester</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token response with access and refresh tokens</returns>
    Task<TokenResponse> GenerateTokensAsync(User user, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access and refresh tokens using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">Current refresh token</param>
    /// <param name="ipAddress">IP address of the requester</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New token response or null if token is invalid</returns>
    Task<TokenResponse?> RefreshTokensAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token to revoke</param>
    /// <param name="ipAddress">IP address of the requester</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Response containing JWT access and refresh tokens.
/// </summary>
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
