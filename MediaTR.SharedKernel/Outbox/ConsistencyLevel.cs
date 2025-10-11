namespace MediaTR.SharedKernel.Outbox;

/// <summary>
/// Defines the consistency level for outbox event processing
/// </summary>
public enum ConsistencyLevel
{
    /// <summary>
    /// Eventual consistency - Events are processed asynchronously in the background (fire-and-forget)
    /// </summary>
    Eventual = 0,

    /// <summary>
    /// Strong consistency - Events are processed immediately within the same request (fire-and-wait)
    /// </summary>
    Strong = 1
}
