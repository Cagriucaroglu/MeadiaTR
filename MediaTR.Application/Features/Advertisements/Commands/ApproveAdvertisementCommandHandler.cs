using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Errors;
using MediaTR.Domain.Events;
using MediaTR.Domain.Events.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using System.Text.Json;

namespace MediaTR.Application.Features.Advertisements.Commands;

/// <summary>
/// ApproveAdvertisementCommand handler with transactional support and fire-and-wait outbox pattern
/// </summary>
internal sealed class ApproveAdvertisementCommandHandler : TransactionalCommandHandlerBase<ApproveAdvertisementCommand, Guid>
{
    private readonly IAdvertisementRepository _advertisementRepository;
    private readonly AdvertisementBusinessLogic _advertisementBusinessLogic;

    public ApproveAdvertisementCommandHandler(
        IServiceProvider serviceProvider,
        IAdvertisementRepository advertisementRepository,
        AdvertisementBusinessLogic advertisementBusinessLogic)
        : base(serviceProvider)
    {
        _advertisementRepository = advertisementRepository;
        _advertisementBusinessLogic = advertisementBusinessLogic;
    }

    protected override async Task<Result<Guid>> ProcessCommandAsync(
        ApproveAdvertisementCommand request,
        CancellationToken cancellationToken)
    {
        // Advertisement'ı repository'den getir
        Advertisement? advertisement = await _advertisementRepository.GetByIdAsync(request.Request.AdvertisementId);

        if (advertisement == null)
            return AdvertisementErrors.NotFound;

        // Business logic ile approve et
        _advertisementBusinessLogic.Approve(advertisement, request.CorrelationId);

        // Repository'ye kaydet
        await _advertisementRepository.UpdateAsync(advertisement);

        // Outbox Event oluştur (Fire-and-Wait Pattern)
        var advertisementPublishedEvent = new AdvertisementPublishedEvent
        {
            Payload = advertisement,
            CorrelationId = request.CorrelationId
        };

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "AdvertisementPublished",
            AggregateId = advertisement.Id,
            AggregateType = nameof(Advertisement),
            Payload = JsonSerializer.Serialize(advertisementPublishedEvent),
            Status = OutboxStatus.Immediate,
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = request.CorrelationId,
            ConsistencyLevel = ConsistencyLevel.Strong
        };

        // OutboxEvent'i BusinessLogicContext'e track et
        request.BusinessLogicContext.TrackOutboxEvent(outboxEvent);

        // OutboxEvent'i database'e ekle (transaction içinde)
        var outboxRepository = Context.GetRepository<OutboxEvent>();
        await outboxRepository.AddAsync(outboxEvent);

        return Result.Success(advertisement.Id);
    }

    /// <summary>
    /// Transaction commit'ten ÖNCE event'leri işle (Fire-and-Wait)
    /// </summary>
    protected override async Task BeforeTransactionCommittedAsync(
        Result<Guid> result,
        BusinessLogicContext blContext,
        CancellationToken cancellationToken)
    {
        if (result.IsSuccess)
        {
            await ProcessOutboxEventsAsync(blContext, cancellationToken);
        }
    }
}