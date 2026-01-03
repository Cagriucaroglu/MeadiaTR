using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Products.Queries.GetFeaturedProducts;

namespace MediaTR.Application.Features.Products.Queries.SearchProducts;

/// <summary>
/// Search products query with pagination (cached for 15 min per page)
/// </summary>
public record SearchProductsQuery(
    string SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQuery<SearchProductsResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record SearchProductsResult(
    IReadOnlyList<ProductSummaryDto> Products,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    string SearchTerm);
