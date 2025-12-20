using MediaTR.Domain.Entities.InMemory;

namespace MediaTR.Domain.Services;

/// <summary>
/// Shopping cart service for Redis-based cart management
/// Supports both authenticated users (userId) and guest users (sessionId)
/// </summary>
public interface IShoppingCartService
{
    // User Cart Operations (authenticated users)
    Task<ShoppingCart> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddItemAsync(Guid userId, Guid productId, int quantity = 1, CancellationToken cancellationToken = default);
    Task UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetCartItemCountAsync(Guid userId, CancellationToken cancellationToken = default);

    // Guest Cart Operations (sessionId-based, no authentication)
    Task<ShoppingCart> GetGuestCartAsync(string sessionId, CancellationToken cancellationToken = default);
    Task AddGuestItemAsync(string sessionId, Guid productId, int quantity = 1, CancellationToken cancellationToken = default);
    Task UpdateGuestItemQuantityAsync(string sessionId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveGuestItemAsync(string sessionId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearGuestCartAsync(string sessionId, CancellationToken cancellationToken = default);

    // Merge guest cart into user cart after login
    Task MergeGuestCartAsync(string guestSessionId, Guid userId, CancellationToken cancellationToken = default);
}
