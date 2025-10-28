using MediaTR.Domain.Events;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MediaTR.Application.Services.OutboxProcessor.OutboxHandlers;

/// <summary>
/// OrderPlaced event handler - Email/SMS notification gönderimi (mock)
/// OptimatePlatform Outbox Pattern implementation
/// </summary>
[OutboxEventHandler("OrderPlaced")]
public sealed class OrderPlacedEventHandler : IOutboxEventHandler
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;

    public OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger)
    {
        _logger = logger;
    }

    public string EventType => "OrderPlaced";

    public async Task<Result> ProcessAsync(
        IOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        try
        {
            _logger.LogInformation(
                "Processing OrderPlaced event {EventId} for aggregate {AggregateType}:{AggregateId}",
                outboxEvent.Id, outboxEvent.AggregateType, outboxEvent.AggregateId);

            // Deserialize payload
            var orderPlacedEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(outboxEvent.Payload);

            if (orderPlacedEvent?.Payload == null)
            {
                _logger.LogError("Failed to deserialize payload for event {EventId}", outboxEvent.Id);
                return Result.Failure(Error.Failure("Outbox.InvalidPayload", "Invalid payload format"));
            }

            var order = orderPlacedEvent.Payload;

            _logger.LogInformation(
                "Processing order: OrderId={OrderId}, UserId={UserId}, TotalAmount={TotalAmount}",
                order.Id,
                order.UserId,
                order.TotalAmount);

            int successCount = 0;

            // MOCK: Email notification
            try
            {
                await SendEmailNotificationAsync(order.Id, order.UserId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification for order {OrderId}", order.Id);
            }

            // MOCK: SMS notification
            try
            {
                await SendSmsNotificationAsync(order.Id, order.UserId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS notification for order {OrderId}", order.Id);
            }

            _logger.LogInformation(
                "Completed processing OrderPlaced event {EventId}. Sent {SuccessCount}/2 notifications",
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
            _logger.LogError(ex, "Unexpected error processing OrderPlaced event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.ProcessingFailed", ex.Message));
        }
    }

    private async Task SendEmailNotificationAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        // MOCK: Email service entegrasyonu
        await Task.Delay(100, cancellationToken); // Simulate network call

        _logger.LogInformation(
            "📧 [MOCK] Email sent to user {UserId} for order {OrderId}",
            userId,
            orderId);
    }

    private async Task SendSmsNotificationAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        // MOCK: SMS service entegrasyonu
        await Task.Delay(100, cancellationToken); // Simulate network call

        _logger.LogInformation(
            "📱 [MOCK] SMS sent to user {UserId} for order {OrderId}",
            userId,
            orderId);
    }
}
