using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MediaTR.Domain.Services;

namespace MediaTR.Application.Features.Wishlist.Commands.ClearWishlist;

/// <summary>
/// ClearWishlistCommand handler
/// MongoDB persistence + Redis cache
/// </summary>
internal sealed class ClearWishlistCommandHandler : IRequestHandler<ClearWishlistCommand, bool>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly WishlistBusinessLogic _wishlistBusinessLogic;
    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly TimeSpan WishlistCacheExpiration = TimeSpan.FromHours(1);

    public ClearWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        WishlistBusinessLogic wishlistBusinessLogic,
        ICacheService cacheService,
        IHttpContextAccessor httpContextAccessor)
    {
        _wishlistRepository = wishlistRepository;
        _wishlistBusinessLogic = wishlistBusinessLogic;
        _cacheService = cacheService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(ClearWishlistCommand request, CancellationToken cancellationToken)
    {
        // Get userId from HttpContext
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User must be authenticated to use wishlist");

        // Get wishlist
        var wishlist = await _wishlistRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wishlist == null)
            return false; // No wishlist = nothing to clear

        // Clear wishlist using BusinessLogic
        _wishlistBusinessLogic.ClearWishlist(wishlist);

        // Update wishlist in MongoDB
        await _wishlistRepository.UpdateAsync(wishlist, cancellationToken: cancellationToken);

        // Update Redis cache
        var cacheKey = GetWishlistCacheKey(userId);
        await _cacheService.SetAsync(cacheKey, wishlist, WishlistCacheExpiration, cancellationToken);

        return true;
    }

    private static string GetWishlistCacheKey(Guid userId) => $"wishlist:{userId}";
}
