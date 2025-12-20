using MediaTR.Domain.Entities;

namespace MediaTR.Domain.Services;

/// <summary>
/// Wishlist service for managing user favorite products
/// Uses MongoDB for persistence + Redis for caching (1 hour TTL)
/// </summary>
public interface IWishlistService
{
    /// <summary>
    /// Get user's wishlist (from cache or DB)
    /// Creates new wishlist if not exists
    /// </summary>
    Task<Wishlist> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add product to wishlist
    /// </summary>
    Task AddProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove product from wishlist
    /// </summary>
    Task RemoveProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if product is in user's wishlist
    /// </summary>
    Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all products in user's wishlist with full product details
    /// </summary>
    Task<IEnumerable<Product>> GetWishlistProductsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all products from wishlist
    /// </summary>
    Task ClearWishlistAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get wishlist item count
    /// </summary>
    Task<int> GetWishlistCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
