using MediaTR.Application.Features.Auth.Commands.Login;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;
using MediaTR.SharedKernel.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MediaTR.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        IDbContext dbContext,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Hash the refresh token to compare with stored hash
        var hashedToken = HashToken(request.RefreshToken);

        // Find refresh token in database
        var refreshToken = await _dbContext.Query<MediaTR.Domain.Entities.RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == hashedToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new InvalidOperationException("Auth.InvalidRefreshToken");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new InvalidOperationException("Auth.UserNotFound");
        }

        // Revoke old refresh token
        await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken, "127.0.0.1", cancellationToken);

        // Generate new tokens
        // Note: IP address should be passed from API layer, using placeholder for now
        var tokens = await _jwtTokenService.GenerateTokensAsync(user, "127.0.0.1", cancellationToken);

        return new LoginResponse(
            user.Id,
            user.UserName,
            user.Email.Value,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshTokenExpiresAt);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }
}
