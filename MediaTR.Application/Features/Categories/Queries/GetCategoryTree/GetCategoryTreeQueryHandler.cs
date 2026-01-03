using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Categories.Queries.GetCategoryTree;

/// <summary>
/// Get category tree query handler (uses Redis cache - 30 min TTL)
/// </summary>
internal sealed class GetCategoryTreeQueryHandler : IQueryHandler<GetCategoryTreeQuery, CategoryTreeResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryTreeResult>> Handle(
        GetCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        // Get all active categories (cached)
        var allCategories = (await _categoryRepository.GetActiveCategoriesAsync(cancellationToken)).ToList();

        // Build category lookup dictionary
        var categoryDict = allCategories.ToDictionary(c => c.Id);

        // Get root categories
        var rootCategories = allCategories
            .Where(c => c.ParentCategoryId == null || c.ParentCategoryId == Guid.Empty)
            .OrderBy(c => c.SortOrder)
            .Select(c => BuildCategoryTree(c, categoryDict))
            .ToList();

        return new CategoryTreeResult(rootCategories);
    }

    /// <summary>
    /// Recursively build category tree with children
    /// </summary>
    private CategoryTreeDto BuildCategoryTree(Category category, Dictionary<Guid, Category> categoryDict)
    {
        // Find all children of this category
        var children = categoryDict.Values
            .Where(c => c.ParentCategoryId == category.Id)
            .OrderBy(c => c.SortOrder)
            .Select(c => BuildCategoryTree(c, categoryDict))
            .ToList();

        return new CategoryTreeDto(
            category.Id,
            category.Name,
            category.Slug,
            category.ImageUrl,
            category.SortOrder,
            children);
    }
}
