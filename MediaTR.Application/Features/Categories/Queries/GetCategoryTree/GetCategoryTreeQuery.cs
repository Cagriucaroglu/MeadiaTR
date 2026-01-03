using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Categories.Queries.GetCategoryTree;

/// <summary>
/// Get category tree query (cached for 30 min)
/// Returns hierarchical category structure with all children
/// </summary>
public record GetCategoryTreeQuery() : IQuery<CategoryTreeResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record CategoryTreeResult(IReadOnlyList<CategoryTreeDto> RootCategories);

/// <summary>
/// Category tree DTO with hierarchical structure
/// </summary>
public record CategoryTreeDto(
    Guid Id,
    string Name,
    string Slug,
    string? ImageUrl,
    int SortOrder,
    IReadOnlyList<CategoryTreeDto> Children);
