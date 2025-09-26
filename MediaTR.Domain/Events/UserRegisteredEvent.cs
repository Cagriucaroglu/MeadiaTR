using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Events;

public class UserRegisteredEvent : IDomainEvent
{
    public Guid UserId { get; }
    public Email Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime OccurredOn { get; }
    public Guid EventId { get; }

    public UserRegisteredEvent(Guid userId, Email email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}