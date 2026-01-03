using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Products.Queries.GetProductBySlug;

/// <summary>
/// Get product by slug query (SEO-friendly URL, cached for 1 hour)
/// </summary>
public record GetProductBySlugQuery(string Slug) : IQuery<GetProductResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
