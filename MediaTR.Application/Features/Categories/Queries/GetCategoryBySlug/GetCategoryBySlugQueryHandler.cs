using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Categories.Queries.GetCategoryBySlug;

/// <summary>
/// Get category by slug query handler (uses Redis cache - 1 hour TTL)
/// </summary>
internal sealed class GetCategoryBySlugQueryHandler : IQueryHandler<GetCategoryBySlugQuery, CategoryDto?>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryBySlugQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto?>> Handle(
        GetCategoryBySlugQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Slug))
            return Error.Validation("Category.InvalidSlug", "Slug cannot be empty");

        // Get from cache or MongoDB
        var category = await _categoryRepository.GetBySlugAsync(request.Slug, cancellationToken);

        if (category == null)
            return (CategoryDto?)null;

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Slug,
            category.ParentCategoryId,
            category.ImageUrl,
            category.SortOrder,
            category.IsActive);
    }
}
