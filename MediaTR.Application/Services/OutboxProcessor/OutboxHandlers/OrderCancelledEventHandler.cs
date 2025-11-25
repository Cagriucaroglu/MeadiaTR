using MediaTR.Domain.Events;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MediaTR.Application.Services.OutboxProcessor.OutboxHandlers;

/// <summary>
/// OrderCancelled event handler - Cancellation notification and refund processing (mock)
/// OptimatePlatform Outbox Pattern implementation
/// </summary>
[OutboxEventHandler("OrderCancelled")]
public sealed class OrderCancelledEventHandler : IOutboxEventHandler
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public string EventType => "OrderCancelled";

    public async Task<Result> ProcessAsync(
        IOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        try
        {
            _logger.LogInformation(
                "Processing OrderCancelled event {EventId} for aggregate {AggregateType}:{AggregateId}",
                outboxEvent.Id, outboxEvent.AggregateType, outboxEvent.AggregateId);

            // Deserialize payload
            var orderCancelledEvent = JsonSerializer.Deserialize<OrderCancelledEvent>(outboxEvent.Payload);

            if (orderCancelledEvent?.Payload == null)
            {
                _logger.LogError("Failed to deserialize payload for event {EventId}", outboxEvent.Id);
                return Result.Failure(Error.Failure("Outbox.InvalidPayload", "Invalid payload format"));
            }

            var order = orderCancelledEvent.Payload;

            _logger.LogInformation(
                "Processing cancelled order: OrderId={OrderId}, UserId={UserId}, TotalAmount={TotalAmount}",
                order.Id,
                order.UserId,
                order.TotalAmount);

            int successCount = 0;

            // MOCK: Cancellation notification
            try
            {
                await SendCancellationNotificationAsync(order.Id, order.UserId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation notification for order {OrderId}", order.Id);
            }

            // MOCK: Refund processing
            try
            {
                await ProcessRefundAsync(order.Id, order.UserId, order.TotalAmount.Amount, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process refund for order {OrderId}", order.Id);
            }

            _logger.LogInformation(
                "Completed processing OrderCancelled event {EventId}. Completed {SuccessCount}/2 operations",
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
            _logger.LogError(ex, "Unexpected error processing OrderCancelled event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.ProcessingFailed", ex.Message));
        }
    }

    private async Task SendCancellationNotificationAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        // MOCK: Email/SMS for order cancellation
        await Task.Delay(100, cancellationToken); // Simulate network call

        _logger.LogInformation(
            "❌ [MOCK] Cancellation notification sent to user {UserId} for order {OrderId}",
            userId,
            orderId);
    }

    private async Task ProcessRefundAsync(Guid orderId, Guid userId, decimal totalAmount, CancellationToken cancellationToken)
    {
        // MOCK: Payment gateway refund processing
        await Task.Delay(150, cancellationToken); // Simulate payment processing

        _logger.LogInformation(
            "💰 [MOCK] Refund processed for user {UserId}, order {OrderId}, amount: {Amount:C}",
            userId,
            orderId,
            totalAmount);
    }
}
