using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Categories.Commands;

/// <summary>
/// CreateCategoryCommand handler with transactional support
/// Note: Category creation doesn't need outbox events (no external side effects)
/// </summary>
internal sealed class CreateCategoryCommandHandler : TransactionalCommandHandlerBase<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly CategoryBusinessLogic _categoryBusinessLogic;

    public CreateCategoryCommandHandler(
        IServiceProvider serviceProvider,
        ICategoryRepository categoryRepository,
        CategoryBusinessLogic categoryBusinessLogic)
        : base(serviceProvider)
    {
        _categoryRepository = categoryRepository;
        _categoryBusinessLogic = categoryBusinessLogic;
    }

    protected override async Task<Result<Guid>> ProcessCommandAsync(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Business logic ile category oluştur
        Category category = _categoryBusinessLogic.CreateCategory(
            request.Request.Name,
            request.Request.Description,
            request.CorrelationId,
            request.Request.ParentCategoryId,
            request.Request.SortOrder
        );

        // Repository'ye kaydet
        await _categoryRepository.AddAsync(category);

        return Result.Success(category.Id);
    }
}