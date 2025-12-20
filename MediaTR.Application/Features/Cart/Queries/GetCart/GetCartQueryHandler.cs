using MediaTR.Domain.Entities.InMemory;
using MediaTR.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Cart.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, ShoppingCart>
{
    private readonly IShoppingCartService _cartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetCartQueryHandler(
        IShoppingCartService cartService,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ShoppingCart> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            // Authenticated user cart
            return await _cartService.GetCartAsync(userId, cancellationToken);
        }
        else
        {
            // Guest cart
            var sessionId = httpContext.Request.Headers["X-Session-Id"].FirstOrDefault();

            if (string.IsNullOrEmpty(sessionId))
            {
                // Generate new session ID and return empty cart
                sessionId = Guid.NewGuid().ToString();
                httpContext.Response.Headers.Append("X-Session-Id", sessionId);
            }

            return await _cartService.GetGuestCartAsync(sessionId, cancellationToken);
        }
    }
}
