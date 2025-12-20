using MediaTR.Domain.Entities.InMemory;
using MediaTR.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Cart.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, ShoppingCart>
{
    private readonly IShoppingCartService _cartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddToCartCommandHandler(
        IShoppingCartService cartService,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ShoppingCart> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        // Check if user is authenticated
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            // Authenticated user cart
            await _cartService.AddItemAsync(userId, request.ProductId, request.Quantity, cancellationToken);
            return await _cartService.GetCartAsync(userId, cancellationToken);
        }
        else
        {
            // Guest cart - use session ID
            var sessionId = httpContext.Request.Headers["X-Session-Id"].FirstOrDefault();

            if (string.IsNullOrEmpty(sessionId))
            {
                // Generate new session ID if not provided
                sessionId = Guid.NewGuid().ToString();
                httpContext.Response.Headers.Append("X-Session-Id", sessionId);
            }

            await _cartService.AddGuestItemAsync(sessionId, request.ProductId, request.Quantity, cancellationToken);
            return await _cartService.GetGuestCartAsync(sessionId, cancellationToken);
        }
    }
}
