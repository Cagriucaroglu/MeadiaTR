using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Products.Queries.GetFeaturedProducts;

namespace MediaTR.Application.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Get products by category query (cached for 30 min)
/// </summary>
public record GetProductsByCategoryQuery(Guid CategoryId) : IQuery<GetProductsByCategoryResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record GetProductsByCategoryResult(
    Guid CategoryId,
    IReadOnlyList<ProductSummaryDto> Products,
    int TotalCount);
