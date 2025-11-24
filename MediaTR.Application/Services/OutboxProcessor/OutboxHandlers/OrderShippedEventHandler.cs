using MediaTR.Domain.Events;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MediaTR.Application.Services.OutboxProcessor.OutboxHandlers;

/// <summary>
/// OrderShipped event handler - Tracking number notification (mock)
/// OptimatePlatform Outbox Pattern implementation
/// </summary>
[OutboxEventHandler("OrderShipped")]
public sealed class OrderShippedEventHandler : IOutboxEventHandler
{
    private readonly ILogger<OrderShippedEventHandler> _logger;

    public OrderShippedEventHandler(ILogger<OrderShippedEventHandler> logger)
    {
        _logger = logger;
    }

    public string EventType => "OrderShipped";

    public async Task<Result> ProcessAsync(
        IOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        try
        {
            _logger.LogInformation(
                "Processing OrderShipped event {EventId} for aggregate {AggregateType}:{AggregateId}",
                outboxEvent.Id, outboxEvent.AggregateType, outboxEvent.AggregateId);

            // Deserialize payload
            var orderShippedEvent = JsonSerializer.Deserialize<OrderShippedEvent>(outboxEvent.Payload);

            if (orderShippedEvent?.Payload == null)
            {
                _logger.LogError("Failed to deserialize payload for event {EventId}", outboxEvent.Id);
                return Result.Failure(Error.Failure("Outbox.InvalidPayload", "Invalid payload format"));
            }

            var order = orderShippedEvent.Payload;

            _logger.LogInformation(
                "Processing shipped order: OrderId={OrderId}, UserId={UserId}, Status={Status}",
                order.Id,
                order.UserId,
                order.Status);

            int successCount = 0;

            // MOCK: Email with tracking information
            try
            {
                await SendTrackingEmailAsync(order.Id, order.UserId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send tracking email for order {OrderId}", order.Id);
            }

            // MOCK: SMS with tracking number
            try
            {
                await SendTrackingSmsAsync(order.Id, order.UserId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send tracking SMS for order {OrderId}", order.Id);
            }

            _logger.LogInformation(
                "Completed processing OrderShipped event {EventId}. Sent {SuccessCount}/2 notifications",
                outboxEvent.Id, successCount);

            // Consider it successful if at least one notification was sent
            return successCount > 0
                ? Result.Success()
                : Result.Failure(Error.Failure("Outbox.AllNotificationsFailed", "All notifications failed to send"));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.DeserializationFailed", $"Deserialization error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing OrderShipped event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.ProcessingFailed", ex.Message));
        }
    }

    private async Task SendTrackingEmailAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        // MOCK: Email service with tracking number
        await Task.Delay(100, cancellationToken); // Simulate network call

        string trackingNumber = $"TRACK-{orderId:N}".Substring(0, 16).ToUpperInvariant();

        _logger.LogInformation(
            "📧 [MOCK] Tracking email sent to user {UserId} for order {OrderId}. Tracking: {TrackingNumber}",
            userId,
            orderId,
            trackingNumber);
    }

    private async Task SendTrackingSmsAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        // MOCK: SMS service with tracking number
        await Task.Delay(100, cancellationToken); // Simulate network call

        string trackingNumber = $"TRACK-{orderId:N}".Substring(0, 16).ToUpperInvariant();

        _logger.LogInformation(
            "📱 [MOCK] Tracking SMS sent to user {UserId} for order {OrderId}. Tracking: {TrackingNumber}",
            userId,
            orderId,
            trackingNumber);
    }
}
