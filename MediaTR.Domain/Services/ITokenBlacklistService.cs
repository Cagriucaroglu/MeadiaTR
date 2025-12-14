namespace MediaTR.Domain.Services;

/// <summary>
/// Token blacklist service for revoking access tokens
/// Uses Redis cache with JTI (JWT ID) based revocation
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Revoke an access token by adding its JTI to the blacklist
    /// Token will be blacklisted until its natural expiration
    /// </summary>
    /// <param name="jti">JWT ID (jti claim from token)</param>
    /// <param name="remainingLifetime">Remaining lifetime of the token (until exp)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeAccessTokenAsync(string jti, TimeSpan remainingLifetime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an access token is revoked
    /// </summary>
    /// <param name="jti">JWT ID (jti claim from token)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is revoked, false otherwise</returns>
    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default);
}
