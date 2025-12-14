using MediaTR.Domain.Services;

namespace MediaTR.Infrastructure.Services.Authentication;

/// <summary>
/// Token blacklist service using Redis cache
/// Revokes access tokens by storing their JTI (JWT ID) in cache
/// </summary>
public sealed class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ICacheService _cacheService;
    private const string BlacklistKeyPrefix = "revoked:jti:";

    public TokenBlacklistService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task RevokeAccessTokenAsync(string jti, TimeSpan remainingLifetime, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw new ArgumentException("JTI cannot be null or empty", nameof(jti));
        }

        // Store JTI in blacklist with expiration matching token's remaining lifetime
        // After token expires naturally, no need to keep it in blacklist
        var key = $"{BlacklistKeyPrefix}{jti}";
        await _cacheService.SetAsync(key, true, remainingLifetime, cancellationToken);
    }

    public async Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return false; // Invalid JTI = not revoked (let other validation catch it)
        }

        var key = $"{BlacklistKeyPrefix}{jti}";
        return await _cacheService.ExistsAsync(key, cancellationToken);
    }
}
