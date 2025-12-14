using System.Text.Json;
using MediaTR.Domain.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace MediaTR.Infrastructure.Services.Cache;

/// <summary>
/// Redis-based distributed cache implementation
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(data))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(data, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15)
        };

        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        await _distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var data = await _distributedCache.GetStringAsync(key, cancellationToken);
        return !string.IsNullOrEmpty(data);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        // Try to get from cache
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Cache miss - create value
        var value = await factory();

        // Store in cache
        await SetAsync(key, value, expiration, cancellationToken);

        return value;
    }
}
