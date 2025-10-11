namespace MediaTR.SharedKernel.Outbox;

/// <summary>
/// Interface for outbox events stored in the database for eventual consistency
/// </summary>
public interface IOutboxEvent
{
    /// <summary>
    /// Unique identifier for the outbox event
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Type of the event (e.g., "OrderPlaced", "ProductCreated")
    /// </summary>
    string EventType { get; set; }

    /// <summary>
    /// ID of the aggregate that generated this event (optional for bulk operations)
    /// </summary>
    Guid? AggregateId { get; set; }

    /// <summary>
    /// Type of the aggregate (e.g., "Order", "Product")
    /// </summary>
    string AggregateType { get; set; }

    /// <summary>
    /// JSON payload containing the event data
    /// </summary>
    string Payload { get; set; }

    /// <summary>
    /// Current processing status of the event
    /// </summary>
    OutboxStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the event was processed (null if not processed yet)
    /// </summary>
    DateTimeOffset? ProcessedAt { get; set; }

    /// <summary>
    /// Number of times this event has been retried
    /// </summary>
    int RetryCount { get; set; }

    /// <summary>
    /// Maximum number of retry attempts allowed
    /// </summary>
    int MaxRetries { get; set; }

    /// <summary>
    /// Last error message if processing failed
    /// </summary>
    string? LastError { get; set; }

    /// <summary>
    /// Correlation ID for request tracking across distributed systems
    /// </summary>
    Guid? CorrelationId { get; set; }

    /// <summary>
    /// Additional metadata for the event (JSON format)
    /// </summary>
    string? Metadata { get; set; }

    /// <summary>
    /// Consistency level for this event (Eventual or Strong)
    /// </summary>
    ConsistencyLevel ConsistencyLevel { get; set; }
}
