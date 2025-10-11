namespace MediaTR.Domain.Events.Entities;

using MediaTR.SharedKernel;
using MediaTR.SharedKernel.Outbox;

/// <summary>
/// Outbox event entity for storing events that need to be processed asynchronously.
/// Implements the Outbox Pattern for eventual consistency in distributed systems.
/// </summary>
public class OutboxEvent : BaseEntity, IOutboxEvent
{
    /// <summary>
    /// Initializes a new instance of the OutboxEvent class with default values.
    /// </summary>
    public OutboxEvent()
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        RetryCount = 0;
        MaxRetries = 5;
        Status = OutboxStatus.Pending;
        ConsistencyLevel = ConsistencyLevel.Eventual;
        EventType = string.Empty;
        AggregateType = string.Empty;
        Payload = string.Empty;
    }

    /// <inheritdoc/>
    public string EventType { get; set; }

    /// <inheritdoc/>
    public Guid? AggregateId { get; set; }

    /// <inheritdoc/>
    public string AggregateType { get; set; }

    /// <inheritdoc/>
    public string Payload { get; set; }

    /// <inheritdoc/>
    public OutboxStatus Status { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset? ProcessedAt { get; set; }

    /// <inheritdoc/>
    public int RetryCount { get; set; }

    /// <inheritdoc/>
    public int MaxRetries { get; set; }

    /// <inheritdoc/>
    public string? LastError { get; set; }

    /// <inheritdoc/>
    public Guid? CorrelationId { get; set; }

    /// <inheritdoc/>
    public string? Metadata { get; set; }

    /// <inheritdoc/>
    public ConsistencyLevel ConsistencyLevel { get; set; }
}
