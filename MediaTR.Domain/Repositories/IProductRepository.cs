using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.Data;

namespace MediaTR.Domain.Repositories;

/// <summary>
/// Paginated search result
/// </summary>
public record ProductSearchResult(
    IReadOnlyList<Product> Products,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search products (no pagination, cached)
    /// </summary>
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search products with pagination (cached per page)
    /// </summary>
    Task<ProductSearchResult> SearchProductsPaginatedAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
}