using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Categories.Commands;

/// <summary>
/// Create category DTO
/// </summary>
public record CreateCategoryDto(
    string Name,
    string Description,
    Guid? ParentCategoryId = null,
    int SortOrder = 0);

/// <summary>
/// Create category command using CommandWrapper pattern
/// </summary>
public sealed record CreateCategoryCommand(
    CreateCategoryDto Request,
    Guid CorrelationId)
    : CommandWrapper<CreateCategoryDto, Guid>(Request, CorrelationId);