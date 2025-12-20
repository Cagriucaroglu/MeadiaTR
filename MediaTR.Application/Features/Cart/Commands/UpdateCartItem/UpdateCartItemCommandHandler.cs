using MediaTR.Domain.Entities.InMemory;
using MediaTR.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, ShoppingCart>
{
    private readonly IShoppingCartService _cartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateCartItemCommandHandler(
        IShoppingCartService cartService,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ShoppingCart> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            // Authenticated user cart
            await _cartService.UpdateItemQuantityAsync(userId, request.ProductId, request.Quantity, cancellationToken);
            return await _cartService.GetCartAsync(userId, cancellationToken);
        }
        else
        {
            // Guest cart
            var sessionId = httpContext.Request.Headers["X-Session-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(sessionId))
                throw new InvalidOperationException("Cart.SessionNotFound");

            await _cartService.UpdateGuestItemQuantityAsync(sessionId, request.ProductId, request.Quantity, cancellationToken);
            return await _cartService.GetGuestCartAsync(sessionId, cancellationToken);
        }
    }
}
