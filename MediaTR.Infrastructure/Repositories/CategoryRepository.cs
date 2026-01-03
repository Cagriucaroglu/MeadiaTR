using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.Data;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MediaTR.Domain.Services;

namespace MediaTR.Infrastructure.Repositories;

public class CategoryRepository : MongoRepository<Category>, ICategoryRepository
{
    private readonly IMongoCollection<Product> _productCollection;
    private readonly ICacheService _cacheService;

    // Cache TTL constants
    private static readonly TimeSpan CategoryCacheTTL = TimeSpan.FromHours(1);
    private static readonly TimeSpan CategoryTreeCacheTTL = TimeSpan.FromMinutes(30);

    public CategoryRepository(
        IOptions<MongoDbSettings> settings,
        ICacheService cacheService) : base(settings)
    {
        _productCollection = _database.GetCollection<Product>("products");
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get category by ID with Redis cache (1 hour TTL)
    /// </summary>
    public override async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCategoryCacheKey(id);

        // Try cache first
        var cachedCategory = await _cacheService.GetAsync<Category>(cacheKey, cancellationToken);
        if (cachedCategory != null)
            return cachedCategory;

        // Cache miss - get from MongoDB
        var category = await base.GetByIdAsync(id, cancellationToken);
        if (category != null)
        {
            await _cacheService.SetAsync(cacheKey, category, CategoryCacheTTL, cancellationToken);
        }

        return category;
    }

    /// <summary>
    /// Get category by slug with Redis cache (1 hour TTL)
    /// </summary>
    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCategorySlugCacheKey(slug);

        // Try cache first
        var cachedCategory = await _cacheService.GetAsync<Category>(cacheKey, cancellationToken);
        if (cachedCategory != null)
            return cachedCategory;

        // Cache miss - get from MongoDB
        var filter = Builders<Category>.Filter.Eq(x => x.Slug, slug);
        var category = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (category != null)
        {
            await _cacheService.SetAsync(cacheKey, category, CategoryCacheTTL, cancellationToken);
        }

        return category;
    }

    /// <summary>
    /// Get root categories with Redis cache (30 min TTL)
    /// </summary>
    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "categories:root";

        // Try cache first
        var cachedCategories = await _cacheService.GetAsync<List<Category>>(cacheKey, cancellationToken);
        if (cachedCategories != null)
            return cachedCategories;

        // Cache miss - get from MongoDB
        var filter = Builders<Category>.Filter.And(
            Builders<Category>.Filter.Or(
                Builders<Category>.Filter.Eq(x => x.ParentCategoryId, (Guid?)null),
                Builders<Category>.Filter.Eq(x => x.ParentCategoryId, Guid.Empty)
            ),
            Builders<Category>.Filter.Eq(x => x.IsActive, true)
        );
        var categories = await _collection.Find(filter)
            .SortBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        // Update cache
        await _cacheService.SetAsync(cacheKey, categories, CategoryTreeCacheTTL, cancellationToken);

        return categories;
    }

    /// <summary>
    /// Get child categories with Redis cache (30 min TTL)
    /// </summary>
    public async Task<IEnumerable<Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetChildCategoriesCacheKey(parentId);

        // Try cache first
        var cachedCategories = await _cacheService.GetAsync<List<Category>>(cacheKey, cancellationToken);
        if (cachedCategories != null)
            return cachedCategories;

        // Cache miss - get from MongoDB
        var filter = Builders<Category>.Filter.And(
            Builders<Category>.Filter.Eq(x => x.ParentCategoryId, parentId),
            Builders<Category>.Filter.Eq(x => x.IsActive, true)
        );
        var categories = await _collection.Find(filter)
            .SortBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        // Update cache
        await _cacheService.SetAsync(cacheKey, categories, CategoryTreeCacheTTL, cancellationToken);

        return categories;
    }

    public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var categories = new List<Category>();
        var currentCategory = await GetByIdAsync(categoryId, cancellationToken);

        while (currentCategory != null)
        {
            categories.Insert(0, currentCategory);
            if (currentCategory.ParentCategoryId == null || currentCategory.ParentCategoryId == Guid.Empty)
                break;
            currentCategory = await GetByIdAsync(currentCategory.ParentCategoryId.Value, cancellationToken);
        }

        return categories;
    }

    /// <summary>
    /// Get active categories with Redis cache (30 min TTL)
    /// </summary>
    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "categories:active";

        // Try cache first
        var cachedCategories = await _cacheService.GetAsync<List<Category>>(cacheKey, cancellationToken);
        if (cachedCategories != null)
            return cachedCategories;

        // Cache miss - get from MongoDB
        var filter = Builders<Category>.Filter.Eq(x => x.IsActive, true);
        var categories = await _collection.Find(filter)
            .SortBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        // Update cache
        await _cacheService.SetAsync(cacheKey, categories, CategoryTreeCacheTTL, cancellationToken);

        return categories;
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(x => x.Slug, slug);

        if (excludeCategoryId.HasValue && excludeCategoryId != Guid.Empty)
        {
            filter = Builders<Category>.Filter.And(
                filter,
                Builders<Category>.Filter.Ne(x => x.Id, excludeCategoryId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }

    public async Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(x => x.ParentCategoryId, categoryId);
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId);
        var count = await _productCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    /// <summary>
    /// Override UpdateAsync to invalidate cache
    /// </summary>
    public override async Task UpdateAsync(Category entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        // Update MongoDB
        await base.UpdateAsync(entity, correlationId, metadata, cancellationToken);

        // Invalidate caches
        await InvalidateCategoryCaches(entity.Id, entity.ParentCategoryId, entity.Slug, cancellationToken);
    }

    /// <summary>
    /// Override AddAsync to invalidate list caches
    /// </summary>
    public override async Task AddAsync(Category entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        // Add to MongoDB
        await base.AddAsync(entity, correlationId, metadata, cancellationToken);

        // Invalidate tree caches (new category added)
        await InvalidateTreeCaches(entity.ParentCategoryId, cancellationToken);
    }

    /// <summary>
    /// Override Delete to invalidate caches
    /// </summary>
    public override void Delete(Category entity, Guid? correlationId = null, string? metadata = null)
    {
        // Delete from MongoDB
        base.Delete(entity, correlationId, metadata);

        // Invalidate caches (fire-and-forget)
        _ = InvalidateCategoryCaches(entity.Id, entity.ParentCategoryId, entity.Slug, CancellationToken.None);
    }

    #region Cache Key Helpers

    private static string GetCategoryCacheKey(Guid categoryId) => $"category:{categoryId}";
    private static string GetCategorySlugCacheKey(string slug) => $"category:slug:{slug}";
    private static string GetChildCategoriesCacheKey(Guid parentId) => $"categories:children:{parentId}";

    #endregion

    #region Cache Invalidation

    private async Task InvalidateCategoryCaches(Guid categoryId, Guid? parentCategoryId, string slug, CancellationToken cancellationToken)
    {
        // Invalidate single category caches
        await _cacheService.RemoveAsync(GetCategoryCacheKey(categoryId), cancellationToken);
        await _cacheService.RemoveAsync(GetCategorySlugCacheKey(slug), cancellationToken);

        // Invalidate tree caches
        await InvalidateTreeCaches(parentCategoryId, cancellationToken);
    }

    private async Task InvalidateTreeCaches(Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        // Invalidate root categories if no parent (or if this affects root)
        if (parentCategoryId == null || parentCategoryId == Guid.Empty)
        {
            await _cacheService.RemoveAsync("categories:root", cancellationToken);
        }
        else
        {
            // Invalidate parent's children cache
            await _cacheService.RemoveAsync(GetChildCategoriesCacheKey(parentCategoryId.Value), cancellationToken);
        }

        // Invalidate active categories cache (affects all tree operations)
        await _cacheService.RemoveAsync("categories:active", cancellationToken);
    }

    #endregion
}