namespace MediaTR.Domain.Services;

/// <summary>
/// Cache service abstraction for distributed caching (Redis)
/// Used for user caching, token blacklist, rate limiting
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get value from cache by key
    /// </summary>
    /// <typeparam name="T">Type of cached object</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached value or default(T) if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    /// <typeparam name="T">Type of object to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Expiration time (default: 15 minutes)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove value from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if key exists, false otherwise</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get value from cache or create it using factory function
    /// </summary>
    /// <typeparam name="T">Type of cached object</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not in cache</param>
    /// <param name="expiration">Expiration time (default: 15 minutes)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached or newly created value</returns>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
}
