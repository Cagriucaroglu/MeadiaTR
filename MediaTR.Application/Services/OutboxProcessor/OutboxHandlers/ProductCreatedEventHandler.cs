using MediaTR.Domain.Events;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MediaTR.Application.Services.OutboxProcessor.OutboxHandlers;

/// <summary>
/// ProductCreated event handler - Cache invalidation and search index update (mock)
/// OptimatePlatform Outbox Pattern implementation
/// </summary>
[OutboxEventHandler("ProductCreated")]
public sealed class ProductCreatedEventHandler : IOutboxEventHandler
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public string EventType => "ProductCreated";

    public async Task<Result> ProcessAsync(
        IOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        try
        {
            _logger.LogInformation(
                "Processing ProductCreated event {EventId} for aggregate {AggregateType}:{AggregateId}",
                outboxEvent.Id, outboxEvent.AggregateType, outboxEvent.AggregateId);

            // Deserialize payload
            var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(outboxEvent.Payload);

            if (productCreatedEvent?.Payload == null)
            {
                _logger.LogError("Failed to deserialize payload for event {EventId}", outboxEvent.Id);
                return Result.Failure(Error.Failure("Outbox.InvalidPayload", "Invalid payload format"));
            }

            var product = productCreatedEvent.Payload;

            _logger.LogInformation(
                "Processing product: ProductId={ProductId}, Name={Name}, CategoryId={CategoryId}",
                product.Id,
                product.Name,
                product.CategoryId);

            int successCount = 0;

            // MOCK: Cache invalidation
            try
            {
                await InvalidateCacheAsync(product.CategoryId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate cache for product {ProductId}", product.Id);
            }

            // MOCK: Search index update
            try
            {
                await UpdateSearchIndexAsync(product.Id, product.Name, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update search index for product {ProductId}", product.Id);
            }

            _logger.LogInformation(
                "Completed processing ProductCreated event {EventId}. Completed {SuccessCount}/2 operations",
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
            _logger.LogError(ex, "Unexpected error processing ProductCreated event {EventId}", outboxEvent.Id);
            return Result.Failure(Error.Failure("Outbox.ProcessingFailed", ex.Message));
        }
    }

    private async Task InvalidateCacheAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        // MOCK: Redis cache invalidation
        await Task.Delay(50, cancellationToken); // Simulate cache operation

        _logger.LogInformation(
            "🗑️ [MOCK] Cache invalidated for category {CategoryId}",
            categoryId);
    }

    private async Task UpdateSearchIndexAsync(Guid productId, string productName, CancellationToken cancellationToken)
    {
        // MOCK: ElasticSearch index update
        await Task.Delay(50, cancellationToken); // Simulate search index operation

        _logger.LogInformation(
            "🔍 [MOCK] Search index updated for product {ProductId} (Name: {ProductName})",
            productId,
            productName);
    }
}
