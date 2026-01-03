using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Products.Queries.GetFeaturedProducts;

/// <summary>
/// Get featured products query (cached for 15 min)
/// </summary>
public record GetFeaturedProductsQuery(int Count = 10) : IQuery<GetFeaturedProductsResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record ProductSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    Money Price,
    bool IsInStock,
    string? MainImageUrl);

public record GetFeaturedProductsResult(
    IReadOnlyList<ProductSummaryDto> Products,
    int TotalCount);
