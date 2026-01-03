using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Categories.Queries.GetCategoryBySlug;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Categories.Queries.GetRootCategories;

/// <summary>
/// Get root categories query handler (uses Redis cache - 30 min TTL)
/// </summary>
internal sealed class GetRootCategoriesQueryHandler : IQueryHandler<GetRootCategoriesQuery, RootCategoriesResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetRootCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<RootCategoriesResult>> Handle(
        GetRootCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        // Get root categories from cache or MongoDB
        var categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);

        var categoryDtos = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.Slug,
            c.ParentCategoryId,
            c.ImageUrl,
            c.SortOrder,
            c.IsActive)).ToList();

        return new RootCategoriesResult(categoryDtos);
    }
}
