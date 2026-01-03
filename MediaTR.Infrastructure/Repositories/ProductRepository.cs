using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.Data;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MediaTR.Domain.Services;

namespace MediaTR.Infrastructure.Repositories;

public class ProductRepository : MongoRepository<Product>, IProductRepository
{
    private readonly ICacheService _cacheService;

    // Cache TTL constants
    private static readonly TimeSpan ProductCacheTTL = TimeSpan.FromHours(1);
    private static readonly TimeSpan CategoryProductsCacheTTL = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ListCacheTTL = TimeSpan.FromMinutes(15);

    public ProductRepository(
        IOptions<MongoDbSettings> settings,
        ICacheService cacheService) : base(settings)
    {
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get product by ID with Redis cache (1 hour TTL)
    /// </summary>
    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetProductCacheKey(id);

        // Try cache first
        var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey, cancellationToken);
        if (cachedProduct != null)
            return cachedProduct;

        // Cache miss - get from MongoDB
        var product = await base.GetByIdAsync(id, cancellationToken);
        if (product != null)
        {
            // Update cache
            await _cacheService.SetAsync(cacheKey, product, ProductCacheTTL, cancellationToken);
        }

        return product;
    }

    /// <summary>
    /// Get products by category ID with Redis cache (30 min TTL)
    /// </summary>
    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCategoryProductsCacheKey(categoryId);

        // Try cache first
        var cachedProducts = await _cacheService.GetAsync<List<Product>>(cacheKey, cancellationToken);
        if (cachedProducts != null)
            return cachedProducts;

        // Cache miss - get from MongoDB
        var filter = Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId);
        var products = await _collection.Find(filter).ToListAsync(cancellationToken);

        // Update cache
        await _cacheService.SetAsync(cacheKey, products, CategoryProductsCacheTTL, cancellationToken);

        return products;
    }

    public async Task<IEnumerable<Product>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Sku, sku);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get product by slug with Redis cache (1 hour TTL)
    /// </summary>
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetProductSlugCacheKey(slug);

        // Try cache first
        var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey, cancellationToken);
        if (cachedProduct != null)
            return cachedProduct;

        // Cache miss - get from MongoDB
        var filter = Builders<Product>.Filter.Eq(x => x.Slug, slug);
        var product = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (product != null)
        {
            // Update cache
            await _cacheService.SetAsync(cacheKey, product, ProductCacheTTL, cancellationToken);
        }

        return product;
    }

    /// <summary>
    /// Get featured products with Redis cache (15 min TTL)
    /// </summary>
    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetFeaturedProductsCacheKey(count);

        // Try cache first
        var cachedProducts = await _cacheService.GetAsync<List<Product>>(cacheKey, cancellationToken);
        if (cachedProducts != null)
            return cachedProducts;

        // Cache miss - get from MongoDB
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Eq(x => x.IsFeatured, true),
            Builders<Product>.Filter.Eq(x => x.IsActive, true)
        );
        var products = await _collection.Find(filter).Limit(count).ToListAsync(cancellationToken);

        // Update cache
        await _cacheService.SetAsync(cacheKey, products, ListCacheTTL, cancellationToken);

        return products;
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        // No cache for admin operations (frequently changing data)
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Gt(x => x.StockQuantity, 0),
            Builders<Product>.Filter.Lte(x => x.StockQuantity, 10),
            Builders<Product>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Search products with Redis cache (15 min TTL) - No pagination
    /// </summary>
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetSearchCacheKey(searchTerm);

        // Try cache first
        var cachedProducts = await _cacheService.GetAsync<List<Product>>(cacheKey, cancellationToken);
        if (cachedProducts != null)
            return cachedProducts;

        // Cache miss - get from MongoDB
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Product>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            ),
            Builders<Product>.Filter.Eq(x => x.IsActive, true)
        );
        var products = await _collection.Find(filter).ToListAsync(cancellationToken);

        // Update cache
        await _cacheService.SetAsync(cacheKey, products, ListCacheTTL, cancellationToken);

        return products;
    }

    /// <summary>
    /// Search products with pagination and Redis cache (15 min TTL per page)
    /// </summary>
    public async Task<ProductSearchResult> SearchProductsPaginatedAsync(
        string searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Max 100 items per page

        var cacheKey = GetSearchPaginatedCacheKey(searchTerm, page, pageSize);

        // Try cache first
        var cachedResult = await _cacheService.GetAsync<ProductSearchResult>(cacheKey, cancellationToken);
        if (cachedResult != null)
            return cachedResult;

        // Cache miss - get from MongoDB
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Product>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            ),
            Builders<Product>.Filter.Eq(x => x.IsActive, true)
        );

        // Get total count
        var totalCount = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        // Calculate pagination
        var skip = (page - 1) * pageSize;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        // Get paginated results
        var products = await _collection
            .Find(filter)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var result = new ProductSearchResult(
            products,
            totalCount,
            page,
            pageSize,
            totalPages);

        // Update cache
        await _cacheService.SetAsync(cacheKey, result, ListCacheTTL, cancellationToken);

        return result;
    }

    /// <summary>
    /// Get active products with Redis cache (15 min TTL)
    /// </summary>
    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "products:active";

        // Try cache first
        var cachedProducts = await _cacheService.GetAsync<List<Product>>(cacheKey, cancellationToken);
        if (cachedProducts != null)
            return cachedProducts;

        // Cache miss - get from MongoDB
        var filter = Builders<Product>.Filter.Eq(x => x.IsActive, true);
        var products = await _collection.Find(filter).ToListAsync(cancellationToken);

        // Update cache
        await _cacheService.SetAsync(cacheKey, products, ListCacheTTL, cancellationToken);

        return products;
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Sku, sku);

        if (excludeProductId.HasValue && excludeProductId != Guid.Empty)
        {
            filter = Builders<Product>.Filter.And(
                filter,
                Builders<Product>.Filter.Ne(x => x.Id, excludeProductId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Slug, slug);

        if (excludeProductId.HasValue && excludeProductId != Guid.Empty)
        {
            filter = Builders<Product>.Filter.And(
                filter,
                Builders<Product>.Filter.Ne(x => x.Id, excludeProductId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }

    /// <summary>
    /// Override UpdateAsync to invalidate cache
    /// </summary>
    public override async Task UpdateAsync(Product entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        // Update MongoDB
        await base.UpdateAsync(entity, correlationId, metadata, cancellationToken);

        // Invalidate caches
        await InvalidateProductCaches(entity.Id, entity.CategoryId, entity.Slug, cancellationToken);
    }

    /// <summary>
    /// Override AddAsync to invalidate list caches
    /// </summary>
    public override async Task AddAsync(Product entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        // Add to MongoDB
        await base.AddAsync(entity, correlationId, metadata, cancellationToken);

        // Invalidate list caches (new product added)
        await InvalidateListCaches(entity.CategoryId, cancellationToken);
    }

    /// <summary>
    /// Override Delete to invalidate caches
    /// </summary>
    public override void Delete(Product entity, Guid? correlationId = null, string? metadata = null)
    {
        // Delete from MongoDB
        base.Delete(entity, correlationId, metadata);

        // Invalidate caches (fire-and-forget)
        _ = InvalidateProductCaches(entity.Id, entity.CategoryId, entity.Slug, CancellationToken.None);
    }

    #region Cache Key Helpers

    private static string GetProductCacheKey(Guid productId) => $"product:{productId}";
    private static string GetProductSlugCacheKey(string slug) => $"product:slug:{slug}";
    private static string GetCategoryProductsCacheKey(Guid categoryId) => $"products:category:{categoryId}";
    private static string GetFeaturedProductsCacheKey(int count) => $"products:featured:{count}";
    private static string GetSearchCacheKey(string searchTerm) => $"products:search:{searchTerm.ToLowerInvariant()}";
    private static string GetSearchPaginatedCacheKey(string searchTerm, int page, int pageSize)
        => $"products:search:{searchTerm.ToLowerInvariant()}:page:{page}:size:{pageSize}";

    #endregion

    #region Cache Invalidation

    private async Task InvalidateProductCaches(Guid productId, Guid categoryId, string slug, CancellationToken cancellationToken)
    {
        // Invalidate single product caches
        await _cacheService.RemoveAsync(GetProductCacheKey(productId), cancellationToken);
        await _cacheService.RemoveAsync(GetProductSlugCacheKey(slug), cancellationToken);

        // Invalidate list caches
        await InvalidateListCaches(categoryId, cancellationToken);
    }

    private async Task InvalidateListCaches(Guid categoryId, CancellationToken cancellationToken)
    {
        // Invalidate category products cache
        await _cacheService.RemoveAsync(GetCategoryProductsCacheKey(categoryId), cancellationToken);

        // Invalidate featured products cache (all count variations - simple approach)
        for (int i = 5; i <= 20; i += 5)
        {
            await _cacheService.RemoveAsync(GetFeaturedProductsCacheKey(i), cancellationToken);
        }

        // Invalidate active products cache
        await _cacheService.RemoveAsync("products:active", cancellationToken);

        // Note: Search cache invalidation is complex (all search terms)
        // We rely on TTL expiration (15 min) for search results
    }

    #endregion
}
