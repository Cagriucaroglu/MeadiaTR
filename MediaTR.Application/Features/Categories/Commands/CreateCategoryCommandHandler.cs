using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Application.Features.Categories.DTOs;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Categories.Commands;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly CategoryBusinessLogic _categoryBusinessLogic;

    public CreateCategoryCommandHandler(CategoryBusinessLogic categoryBusinessLogic)
    {
        _categoryBusinessLogic = categoryBusinessLogic;
    }

    public async Task<Result<CreateCategoryResult>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
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

        // Return result using implicit operator
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