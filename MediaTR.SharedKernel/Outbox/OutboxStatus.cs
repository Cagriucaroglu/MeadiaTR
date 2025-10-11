namespace MediaTR.SharedKernel.Outbox;

/// <summary>
/// Outbox event processing status
/// </summary>
public enum OutboxStatus
{
    /// <summary>
    /// Event is pending and waiting to be processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Event is currently being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Event has been successfully processed
    /// </summary>
    Processed = 2,

    /// <summary>
    /// Event processing failed and will not be retried
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Event processing failed but will be retried
    /// </summary>
    Retrying = 4,

    /// <summary>
    /// Event should be processed immediately in fire-and-wait mode
    /// </summary>
    Immediate = 5
}
