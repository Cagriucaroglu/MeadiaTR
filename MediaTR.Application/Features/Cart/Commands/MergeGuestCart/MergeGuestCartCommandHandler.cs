using MediaTR.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Cart.Commands.MergeGuestCart;

public class MergeGuestCartCommandHandler : IRequestHandler<MergeGuestCartCommand, bool>
{
    private readonly IShoppingCartService _cartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MergeGuestCartCommandHandler(
        IShoppingCartService cartService,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(MergeGuestCartCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        // Must be authenticated to merge
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("User must be authenticated to merge cart");

        await _cartService.MergeGuestCartAsync(request.GuestSessionId, userId, cancellationToken);

        return true;
    }
}
