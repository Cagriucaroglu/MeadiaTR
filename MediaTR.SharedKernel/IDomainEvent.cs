using MediatR;

namespace MediaTR.SharedKernel;

public interface IDomainEvent : INotification
{
    string EventName { get; }
    DateTimeOffset OccurredOn { get; }
    Guid OccurredBy { get; }
    Guid CorrelationId { get; }
    string Version { get; }
}

public interface IDomainEvent<out T> : IDomainEvent
{
    T Payload { get; }
    string PayloadType => typeof(T).Name;
}

public abstract record DomainEvent<T> : IDomainEvent<T>
{
    public string EventName => GetType().Name;
    public required T Payload { get; init; }
    public required Guid CorrelationId { get; init; }
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public Guid OccurredBy { get; init; } = Guid.Empty; // TODO: Get from current user context
    public string Version { get; init; } = "1.0.0";
}