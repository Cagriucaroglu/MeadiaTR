using MediatR;

namespace MediaTR.Application.Features.Auth.Commands.Login;

/// <summary>
/// Login with email/username and password.
/// </summary>
public record LoginCommand(
    string EmailOrUsername,
    string Password,
    bool RememberMe = false) : IRequest<LoginResponse>;

/// <summary>
/// Response containing user information and authentication tokens.
/// </summary>
public record LoginResponse(
    Guid UserId,
    string UserName,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
