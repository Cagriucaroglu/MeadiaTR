using MediatR;

namespace MediaTR.Application.Features.Advertisements.Commands;

public record ApproveAdvertisementCommand(Guid AdvertisementId) : IRequest<ApproveAdvertisementResult>;

public record ApproveAdvertisementResult(
    Guid Id,
    string Status,
    DateTime? PublishedAt,
    DateTime? ExpiresAt
);