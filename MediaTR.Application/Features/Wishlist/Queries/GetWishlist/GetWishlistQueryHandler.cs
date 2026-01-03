using MediatR;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace MediaTR.Application.Features.Wishlist.Queries.GetWishlist;

/// <summary>
/// GetWishlistQuery handler
/// Redis cache + MongoDB fallback
/// </summary>
internal sealed class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistDto?>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly TimeSpan WishlistCacheExpiration = TimeSpan.FromHours(1);

    public GetWishlistQueryHandler(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository,
        ICacheService cacheService,
        IHttpContextAccessor httpContextAccessor)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<WishlistDto?> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        // Get userId from HttpContext
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User must be authenticated to use wishlist");

        // Try cache first
        var cacheKey = GetWishlistCacheKey(userId);
        var cachedWishlist = await _cacheService.GetAsync<Domain.Entities.Wishlist>(cacheKey, cancellationToken);

        Domain.Entities.Wishlist? wishlist;
        if (cachedWishlist != null)
        {
            wishlist = cachedWishlist;
        }
        else
        {
            // Cache miss - get from MongoDB
            wishlist = await _wishlistRepository.GetByUserIdAsync(userId, cancellationToken);
            if (wishlist == null)
                return null; // No wishlist found

            // Update cache
            await _cacheService.SetAsync(cacheKey, wishlist, WishlistCacheExpiration, cancellationToken);
        }

        // Get product details
        if (wishlist.ProductIds.Count == 0)
        {
            return new WishlistDto(
                wishlist.Id,
                wishlist.UserId,
                Array.Empty<WishlistProductDto>(),
                0,
                wishlist.CreatedAt,
                wishlist.UpdatedAt ?? DateTimeOffset.Now);
        }

        // Fetch all products in parallel
        var productTasks = wishlist.ProductIds.Select(productId =>
            _productRepository.GetByIdAsync(productId, cancellationToken));
        var products = await Task.WhenAll(productTasks);

        // Map to DTOs (filter out null products)
        var productDtos = products
            .Where(p => p != null)
            .Select(p => new WishlistProductDto(
                p!.Id,
                p.Name,
                p.Description,
                p.Price.Amount,
                p.MainImageUrl,
                p.IsActive,
                p.StockQuantity))
            .ToList();

        return new WishlistDto(
            wishlist.Id,
            wishlist.UserId,
            productDtos,
            productDtos.Count,
            wishlist.CreatedAt,
            wishlist.UpdatedAt ?? DateTimeOffset.Now);
    }

    private static string GetWishlistCacheKey(Guid userId) => $"wishlist:{userId}";
}
