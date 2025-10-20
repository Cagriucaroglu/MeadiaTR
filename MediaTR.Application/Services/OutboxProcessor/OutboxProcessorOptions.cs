namespace MediaTR.Application.Services.OutboxProcessor;

/// <summary>
/// Configuration options for the Outbox Pattern background processor.
/// Controls polling intervals, batch sizes, and retry behavior.
/// </summary>
public sealed class OutboxProcessorOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "OutboxProcessor";

    /// <summary>
    /// How often to poll for pending outbox events (in seconds).
    /// Default: 5 seconds.
    /// </summary>
    public int PollingIntervalSeconds { get; init; } = 5;

    /// <summary>
    /// Maximum number of events to process in a single batch.
    /// Default: 10.
    /// </summary>
    public int BatchSize { get; init; } = 10;

    /// <summary>
    /// Default threshold in seconds before Immediate events are considered stale.
    /// Default: 30 seconds.
    /// </summary>
    public int DefaultStaleThresholdSeconds { get; init; } = 30;
}
