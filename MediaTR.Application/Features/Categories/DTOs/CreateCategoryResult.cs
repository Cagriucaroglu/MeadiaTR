namespace MediaTR.Application.Features.Categories.DTOs;

public record CreateCategoryResult(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    Guid? ParentCategoryId,
    int SortOrder,
    bool IsActive
);