namespace MediaTR.SharedKernel.Outbox;

/// <summary>
/// Attribute to mark outbox event handlers with their event type.
/// Used for automatic handler discovery and registration via keyed services.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class OutboxEventHandlerAttribute : Attribute
{
    /// <summary>
    /// The event type that this handler processes.
    /// </summary>
    public string EventType { get; }

    /// <summary>
    /// Initializes a new instance of the OutboxEventHandlerAttribute.
    /// </summary>
    /// <param name="eventType">The event type that this handler processes</param>
    public OutboxEventHandlerAttribute(string eventType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        EventType = eventType;
    }
}
