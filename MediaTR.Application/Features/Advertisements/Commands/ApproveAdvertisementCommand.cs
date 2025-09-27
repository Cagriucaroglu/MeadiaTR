using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Advertisements.Commands;

public record ApproveAdvertisementCommand(Guid AdvertisementId) : ICommand<ApproveAdvertisementResult>;

public record ApproveAdvertisementResult(
    Guid Id,
    string Status,
    DateTime? PublishedAt,
    DateTime? ExpiresAt
);