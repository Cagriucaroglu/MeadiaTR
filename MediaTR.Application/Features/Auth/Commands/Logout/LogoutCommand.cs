using MediatR;

namespace MediaTR.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Logout by revoking the refresh token.
/// </summary>
public record LogoutCommand(
    string RefreshToken) : IRequest<bool>;
