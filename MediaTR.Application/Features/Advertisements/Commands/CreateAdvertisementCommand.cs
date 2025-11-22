using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Advertisements.Commands;

/// <summary>
/// Create advertisement DTO
/// </summary>
public record CreateAdvertisementDto(
    string Title,
    string Description,
    Guid ProductId,
    Guid SellerId,
    Money Price,
    bool IsNegotiable = false,
    bool IsUrgent = false,
    string? ContactPhone = null,
    string? ContactEmail = null);

/// <summary>
/// Create advertisement command using CommandWrapper pattern
/// </summary>
public sealed record CreateAdvertisementCommand(
    CreateAdvertisementDto Request,
    Guid CorrelationId)
    : CommandWrapper<CreateAdvertisementDto, Guid>(Request, CorrelationId);