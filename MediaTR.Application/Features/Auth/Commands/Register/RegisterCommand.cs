using MediatR;

namespace MediaTR.Application.Features.Auth.Commands.Register;

/// <summary>
/// Register a new user account.
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string UserName,
    string? PhoneNumber) : IRequest<RegisterResponse>;

/// <summary>
/// Response containing user ID and authentication tokens.
/// </summary>
public record RegisterResponse(
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
