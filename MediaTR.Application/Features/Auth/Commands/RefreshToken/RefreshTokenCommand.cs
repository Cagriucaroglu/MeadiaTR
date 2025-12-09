using MediaTR.Application.Features.Auth.Commands.Login;
using MediatR;

namespace MediaTR.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Refresh access and refresh tokens using a valid refresh token.
/// </summary>
public record RefreshTokenCommand(
    string RefreshToken) : IRequest<LoginResponse>;
