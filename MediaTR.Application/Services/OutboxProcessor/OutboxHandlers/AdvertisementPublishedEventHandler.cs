using MediaTR.Domain.Events;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MediaTR.Application.Services.OutboxProcessor.OutboxHandlers;

/// <summary>
/// AdvertisementPublished event handler - Seller notification and analytics tracking (mock)
/// OptimatePlatform Outbox Pattern implementation
/// </summary>
[OutboxEventHandler("AdvertisementPublished")]
public sealed class AdvertisementPublishedEventHandler : IOutboxEventHandler
{
    private readonly ILogger<AdvertisementPublishedEventHandler> _logger;

    public AdvertisementPublishedEventHandler(ILogger<AdvertisementPublishedEventHandler> logger)
    {
        _logger = logger;
    }

    public string EventType => "AdvertisementPublished";

    public async Task<Result> ProcessAsync(
        IOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        try
        {
            _logger.LogInformation(
                "Processing AdvertisementPublished event {EventId} for aggregate {AggregateType}:{AggregateId}",
                outboxEvent.Id, outboxEvent.AggregateType, outboxEvent.AggregateId);

            // Deserialize payload
            var advertisementPublishedEvent = JsonSerializer.Deserialize<AdvertisementPublishedEvent>(outboxEvent.Payload);

            if (advertisementPublishedEvent?.Payload == null)
            {
                _logger.LogError("Failed to deserialize payload for event {EventId}", outboxEvent.Id);
                return Result.Failure(Error.Failure("Outbox.InvalidPayload", "Invalid payload format"));
            }

            var advertisement = advertisementPublishedEvent.Payload;

            _logger.LogInformation(
                "Processing advertisement: AdId={AdId}, ProductId={ProductId}, SellerId={SellerId}",
                advertisement.Id,
                advertisement.ProductId,
                advertisement.SellerId);

            int successCount = 0;

            // MOCK: Email notification to seller
            try
            {
                await SendSellerNotificationAsync(advertisement.Id, advertisement.SellerId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send seller notification for advertisement {AdId}", advertisement.Id);
            }

            // MOCK: Analytics tracking
            try
            {
                await TrackAnalyticsAsync(advertisement.Id, advertisement.ProductId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track analytics for advertisement {AdId}", advertisement.Id);
            }

            _logger.LogInformation(
                "Completed processing AdvertisementPublished event {EventId}. Completed {SuccessCount}/2 operations",
                outboxEvent.Id, successCount);

            // Consider it successful if at least one operation succeeded
            return successCount > 0
                ? Result.Success()
                : Result.Failure(Error.Failure("Outbox.AllOperationsFailed", "All operations failed"));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.DeserializationFailed", $"Deserialization error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing AdvertisementPublished event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.ProcessingFailed", ex.Message));
        }
    }

    private async Task SendSellerNotificationAsync(Guid advertisementId, Guid sellerId, CancellationToken cancellationToken)
    {
        // MOCK: Email service to notify seller
        await Task.Delay(100, cancellationToken); // Simulate network call

        _logger.LogInformation(
            "📧 [MOCK] Seller notification sent to {SellerId} for advertisement {AdId}",
            sellerId,
            advertisementId);
    }

    private async Task TrackAnalyticsAsync(Guid advertisementId, Guid productId, CancellationToken cancellationToken)
    {
        // MOCK: Analytics tracking (Google Analytics, Mixpanel, etc.)
        await Task.Delay(50, cancellationToken); // Simulate analytics call

        _logger.LogInformation(
            "📊 [MOCK] Analytics tracked for advertisement {AdId}, product {ProductId}",
            advertisementId,
            productId);
    }
}
