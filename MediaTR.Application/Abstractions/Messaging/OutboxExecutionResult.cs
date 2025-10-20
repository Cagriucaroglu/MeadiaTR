using MediaTR.SharedKernel;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Abstractions.Messaging;

/// <summary>
/// Container for results of fire-and-wait OutboxEvent processing
/// </summary>
public sealed class OutboxExecutionResult
{
    private readonly List<(IOutboxEvent Event, Result Result)> results = [];

    /// <summary>
    /// All processed OutboxEvents with their results
    /// </summary>
    public IReadOnlyList<(IOutboxEvent Event, Result Result)> Results => results.AsReadOnly();

    /// <summary>
    /// Returns true if all events were processed successfully
    /// </summary>
    public bool AllSucceeded => results.TrueForAll(r => r.Result.IsSuccess);

    /// <summary>
    /// Returns true if any event failed processing
    /// </summary>
    public bool AnyFailed => results.Exists(r => r.Result.IsFailure);

    /// <summary>
    /// Gets all errors from failed events
    /// </summary>
    public IEnumerable<Error> GetErrors() =>
        results.Where(r => r.Result.IsFailure)
               .Select(r => r.Result.Error);

    /// <summary>
    /// Gets the result for a specific event by ID
    /// </summary>
    public Result? GetResultByEventId(Guid eventId) =>
        results.Find(r => r.Event.Id == eventId).Result;

    /// <summary>
    /// Gets all successfully processed events
    /// </summary>
    public IEnumerable<IOutboxEvent> GetSuccessfulEvents() =>
        results.Where(r => r.Result.IsSuccess)
               .Select(r => r.Event);

    /// <summary>
    /// Gets all failed events
    /// </summary>
    public IEnumerable<IOutboxEvent> GetFailedEvents() =>
        results.Where(r => r.Result.IsFailure)
               .Select(r => r.Event);

    /// <summary>
    /// Adds a result to the collection
    /// </summary>
    internal void AddResult(IOutboxEvent evt, Result result)
    {
        ArgumentNullException.ThrowIfNull(evt);
        ArgumentNullException.ThrowIfNull(result);
        results.Add((evt, result));
    }
}
