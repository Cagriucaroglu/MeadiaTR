using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Services;
using MediaTR.Infrastructure.Configuration;
using MediaTR.Infrastructure.Data;
using MediaTR.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MediaTR.Infrastructure.Services.Authentication;

/// <summary>
/// JWT token service implementation using HS256 algorithm.
/// Manages access tokens (15 min) and refresh tokens (7 days) with rotation strategy.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private const int MaxConcurrentDevices = 5;
    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(
        ApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<TokenResponse> GenerateTokensAsync(
        User user,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var jti = Guid.NewGuid().ToString();
        var accessToken = GenerateAccessToken(user, jti);
        var refreshTokenValue = GenerateRefreshTokenValue();

        var accessTokenExpiresAt = _dateTimeProvider.OffsetUtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = _dateTimeProvider.OffsetUtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        // Hash refresh token before storing (SHA256)
        var hashedRefreshToken = HashToken(refreshTokenValue);

        // Create refresh token entity
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = hashedRefreshToken,
            UserId = user.Id,
            JwtId = jti,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedByIp = ipAddress,
            CreatedAt = _dateTimeProvider.OffsetUtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);

        // Clean up old refresh tokens (keep only latest 5 per user)
        await CleanupOldRefreshTokensAsync(user.Id, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken,
            refreshTokenValue, // Return unhashed token to client
            accessTokenExpiresAt,
            refreshTokenExpiresAt);
    }

    public async Task<TokenResponse?> RefreshTokensAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var hashedToken = HashToken(refreshToken);

        // Find refresh token in database
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == hashedToken, cancellationToken);

        if (storedToken == null || !storedToken.IsActive)
            return null;

        // Revoke old token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = _dateTimeProvider.OffsetUtcNow;
        storedToken.RevokedByIp = ipAddress;

        // Note: User is not in SQL Server, it's in MongoDB
        // We'll fetch it in the handler layer, not here
        // For now, return null - will be implemented in Phase B

        await _dbContext.SaveChangesAsync(cancellationToken);

        return null; // TODO: Implement in Phase B with UserRepository
    }

    public async Task RevokeRefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var hashedToken = HashToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == hashedToken, cancellationToken);

        if (storedToken == null || storedToken.IsRevoked)
            return;

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = _dateTimeProvider.OffsetUtcNow;
        storedToken.RevokedByIp = ipAddress;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private string GenerateAccessToken(User user, string jti)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("username", user.UserName),
            new Claim("fullName", user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = _dateTimeProvider.OffsetUtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime, // JWT library expects DateTime
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshTokenValue()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task CleanupOldRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Get all non-revoked tokens for user, ordered by creation date
        var userTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);

        // If more than max allowed, revoke oldest ones
        if (userTokens.Count >= MaxConcurrentDevices)
        {
            var tokensToRevoke = userTokens.Skip(MaxConcurrentDevices - 1).ToList();
            foreach (var token in tokensToRevoke)
            {
                token.IsRevoked = true;
                token.RevokedAt = _dateTimeProvider.OffsetUtcNow;
                token.RevokedByIp = "system-cleanup";
            }
        }
    }
}
