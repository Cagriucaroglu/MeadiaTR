using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Advertisements.Commands;

public record ApproveAdvertisementCommand(Guid AdvertisementId) : ICommand<ApproveAdvertisementResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
};

public record ApproveAdvertisementResult(
    Guid Id,
    string Status,
    DateTime? PublishedAt,
    DateTime? ExpiresAt
);