using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Categories.Queries.GetCategoryBySlug;

namespace MediaTR.Application.Features.Categories.Queries.GetChildCategories;

/// <summary>
/// Get child categories query (cached for 30 min)
/// Returns direct children of a parent category
/// </summary>
public record GetChildCategoriesQuery(Guid ParentId) : IQuery<ChildCategoriesResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record ChildCategoriesResult(
    Guid ParentId,
    IReadOnlyList<CategoryDto> Children);
