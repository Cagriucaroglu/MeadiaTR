using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.Data;

namespace MediaTR.Domain.Repositories;

/// <summary>
/// Wishlist repository interface for MongoDB operations
/// </summary>
public interface IWishlistRepository : IRepository<Wishlist>
{
    /// <summary>
    /// Get wishlist by user ID
    /// </summary>
    Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has a wishlist
    /// </summary>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
