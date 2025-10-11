namespace MediaTR.Domain.Errors;

using MediaTR.SharedKernel.ResultAndError;

/// <summary>
/// Contains error definitions for outbox pattern operations
/// </summary>
public static class OutboxErrors
{
    /// <summary>
    /// Error when OutboxProcessor is not available
    /// </summary>
    public static Error ProcessorNotFound() => Error.Failure(
        "Outbox.ProcessorNotFound",
        "OutboxProcessor not available - event deferred to background processing");

    /// <summary>
    /// Error when event processing fails
    /// </summary>
    public static Error ProcessingFailed(string eventType, string message) => Error.Failure(
        "Outbox.ProcessingFailed",
        $"Failed to process outbox event of type '{eventType}': {message}");

    /// <summary>
    /// Error when event payload is invalid
    /// </summary>
    public static Error InvalidPayload(Guid eventId) => Error.Validation(
        "Outbox.InvalidPayload",
        $"Invalid payload for outbox event {eventId}");

    /// <summary>
    /// Error when no handler is found for event type
    /// </summary>
    public static Error HandlerNotFound(string eventType) => Error.NotFound(
        "Outbox.HandlerNotFound",
        $"No handler found for event type '{eventType}'");

    /// <summary>
    /// Error when maximum retry count is exceeded
    /// </summary>
    public static Error MaxRetriesExceeded(Guid eventId, int maxRetries) => Error.Failure(
        "Outbox.MaxRetriesExceeded",
        $"Outbox event {eventId} exceeded maximum retry count of {maxRetries}");
}
