using MediatR;

namespace MediaTR.SharedKernel;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}