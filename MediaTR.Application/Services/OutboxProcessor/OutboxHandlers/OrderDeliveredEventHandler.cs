using MediaTR.Domain.Events;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MediaTR.Application.Services.OutboxProcessor.OutboxHandlers;

/// <summary>
/// OrderDelivered event handler - Delivery confirmation and review request (mock)
/// OptimatePlatform Outbox Pattern implementation
/// </summary>
[OutboxEventHandler("OrderDelivered")]
public sealed class OrderDeliveredEventHandler : IOutboxEventHandler
{
    private readonly ILogger<OrderDeliveredEventHandler> _logger;

    public OrderDeliveredEventHandler(ILogger<OrderDeliveredEventHandler> logger)
    {
        _logger = logger;
    }

    public string EventType => "OrderDelivered";

    public async Task<Result> ProcessAsync(
        IOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        try
        {
            _logger.LogInformation(
                "Processing OrderDelivered event {EventId} for aggregate {AggregateType}:{AggregateId}",
                outboxEvent.Id, outboxEvent.AggregateType, outboxEvent.AggregateId);

            // Deserialize payload
            var orderDeliveredEvent = JsonSerializer.Deserialize<OrderDeliveredEvent>(outboxEvent.Payload);

            if (orderDeliveredEvent?.Payload == null)
            {
                _logger.LogError("Failed to deserialize payload for event {EventId}", outboxEvent.Id);
                return Result.Failure(Error.Failure("Outbox.InvalidPayload", "Invalid payload format"));
            }

            var order = orderDeliveredEvent.Payload;

            _logger.LogInformation(
                "Processing delivered order: OrderId={OrderId}, UserId={UserId}, Status={Status}",
                order.Id,
                order.UserId,
                order.Status);

            int successCount = 0;

            // MOCK: Delivery confirmation notification
            try
            {
                await SendDeliveryConfirmationAsync(order.Id, order.UserId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send delivery confirmation for order {OrderId}", order.Id);
            }

            // MOCK: Review request email
            try
            {
                await SendReviewRequestAsync(order.Id, order.UserId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send review request for order {OrderId}", order.Id);
            }

            _logger.LogInformation(
                "Completed processing OrderDelivered event {EventId}. Sent {SuccessCount}/2 notifications",
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
            _logger.LogError(ex, "Unexpected error processing OrderDelivered event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.ProcessingFailed", ex.Message));
        }
    }

    private async Task SendDeliveryConfirmationAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        // MOCK: Email/SMS for delivery confirmation
        await Task.Delay(100, cancellationToken); // Simulate network call

        _logger.LogInformation(
            "✅ [MOCK] Delivery confirmation sent to user {UserId} for order {OrderId}",
            userId,
            orderId);
    }

    private async Task SendReviewRequestAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        // MOCK: Review request email (delayed by 1-2 days in production)
        await Task.Delay(100, cancellationToken); // Simulate network call

        _logger.LogInformation(
            "⭐ [MOCK] Review request sent to user {UserId} for order {OrderId}",
            userId,
            orderId);
    }
}
