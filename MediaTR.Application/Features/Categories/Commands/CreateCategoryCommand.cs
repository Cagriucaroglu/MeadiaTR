using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Categories.DTOs;

namespace MediaTR.Application.Features.Categories.Commands;

public record CreateCategoryCommand(
    string Name,
    string Description,
    Guid? ParentCategoryId = null,
    int SortOrder = 0
) : ICommand<CreateCategoryResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
};