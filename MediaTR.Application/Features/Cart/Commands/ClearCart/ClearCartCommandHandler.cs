using MediaTR.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Cart.Commands.ClearCart;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, bool>
{
    private readonly IShoppingCartService _cartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClearCartCommandHandler(
        IShoppingCartService cartService,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            await _cartService.ClearCartAsync(userId, cancellationToken);
        }
        else
        {
            var sessionId = httpContext.Request.Headers["X-Session-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(sessionId))
            {
                await _cartService.ClearGuestCartAsync(sessionId, cancellationToken);
            }
        }

        return true;
    }
}
