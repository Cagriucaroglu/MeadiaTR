using MediaTR.Domain.Services;
using MediatR;

namespace MediaTR.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IJwtTokenService _jwtTokenService;

    public LogoutCommandHandler(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Note: IP address should be passed from API layer, using placeholder for now
        await _jwtTokenService.RevokeRefreshTokenAsync(
            request.RefreshToken,
            "127.0.0.1",
            cancellationToken);

        return true;
    }
}
