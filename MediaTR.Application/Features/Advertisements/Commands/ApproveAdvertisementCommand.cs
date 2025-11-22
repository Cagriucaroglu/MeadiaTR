using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Advertisements.Commands;

/// <summary>
/// Approve advertisement DTO
/// </summary>
public record ApproveAdvertisementDto(Guid AdvertisementId);

/// <summary>
/// Approve advertisement command using CommandWrapper pattern
/// </summary>
public sealed record ApproveAdvertisementCommand(
    ApproveAdvertisementDto Request,
    Guid CorrelationId)
    : CommandWrapper<ApproveAdvertisementDto, Guid>(Request, CorrelationId);