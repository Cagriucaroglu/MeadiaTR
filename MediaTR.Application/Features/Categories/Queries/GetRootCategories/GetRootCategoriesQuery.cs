using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Categories.Queries.GetCategoryBySlug;

namespace MediaTR.Application.Features.Categories.Queries.GetRootCategories;

/// <summary>
/// Get root categories query (cached for 30 min)
/// Returns only top-level categories (no children included)
/// </summary>
public record GetRootCategoriesQuery() : IQuery<RootCategoriesResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record RootCategoriesResult(IReadOnlyList<CategoryDto> Categories);
