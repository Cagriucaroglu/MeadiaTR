using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Categories.Queries.GetCategoryBySlug;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Categories.Queries.GetChildCategories;

/// <summary>
/// Get child categories query handler (uses Redis cache - 30 min TTL)
/// </summary>
internal sealed class GetChildCategoriesQueryHandler : IQueryHandler<GetChildCategoriesQuery, ChildCategoriesResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetChildCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ChildCategoriesResult>> Handle(
        GetChildCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ParentId == Guid.Empty)
            return Error.Validation("Category.InvalidParentId", "Parent ID cannot be empty");

        // Get child categories from cache or MongoDB
        var categories = await _categoryRepository.GetChildCategoriesAsync(request.ParentId, cancellationToken);

        var categoryDtos = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.Slug,
            c.ParentCategoryId,
            c.ImageUrl,
            c.SortOrder,
            c.IsActive)).ToList();

        return new ChildCategoriesResult(request.ParentId, categoryDtos);
    }
}
