using MediaTR.Domain.Entities.InMemory;
using MediaTR.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, ShoppingCart>
{
    private readonly IShoppingCartService _cartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RemoveFromCartCommandHandler(
        IShoppingCartService cartService,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ShoppingCart> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            await _cartService.RemoveItemAsync(userId, request.ProductId, cancellationToken);
            return await _cartService.GetCartAsync(userId, cancellationToken);
        }
        else
        {
            var sessionId = httpContext.Request.Headers["X-Session-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(sessionId))
                throw new InvalidOperationException("Cart.SessionNotFound");

            await _cartService.RemoveGuestItemAsync(sessionId, request.ProductId, cancellationToken);
            return await _cartService.GetGuestCartAsync(sessionId, cancellationToken);
        }
    }
}
