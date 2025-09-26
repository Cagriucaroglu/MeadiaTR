using MediatR;
using MediaTR.Application.BusinessLogic;
using MediaTR.Application.Features.Categories.DTOs;

namespace MediaTR.Application.Features.Categories.Commands;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly CategoryBusinessLogic _categoryBusinessLogic;

    public CreateCategoryCommandHandler(CategoryBusinessLogic categoryBusinessLogic)
    {
        _categoryBusinessLogic = categoryBusinessLogic;
    }

    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Delegate to business logic
        var category = _categoryBusinessLogic.CreateCategory(
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.SortOrder
        );

        // TODO: Save to repository
        // await _categoryRepository.AddAsync(category, cancellationToken);

        // Return result
        return new CreateCategoryResult(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentCategoryId,
            category.SortOrder,
            category.IsActive
        );
    }
}