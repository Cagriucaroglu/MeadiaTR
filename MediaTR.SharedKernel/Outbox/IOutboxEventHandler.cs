#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

namespace MediaTR.SharedKernel.Outbox;

using MediaTR.SharedKernel.ResultAndError;

/// <summary>
/// Base interface for all outbox event handlers.
/// Handlers must be decorated with [OutboxEventHandler("EventType")] attribute for DI registration.
/// </summary>
public interface IOutboxEventHandler
{
    /// <summary>
    /// Event type that this handler processes.
    /// Must match the value in [OutboxEventHandler] attribute.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Processes the outbox event asynchronously.
    /// </summary>
    /// <param name="outboxEvent">The outbox event to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure with errors</returns>
    Task<Result> ProcessAsync(IOutboxEvent outboxEvent, CancellationToken cancellationToken = default);
}
