using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Categories.Queries.GetCategoryBySlug;

/// <summary>
/// Get category by slug query (cached for 1 hour)
/// </summary>
public record GetCategoryBySlugQuery(string Slug) : IQuery<CategoryDto?>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId,
    string? ImageUrl,
    int SortOrder,
    bool IsActive);
