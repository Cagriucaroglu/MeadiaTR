using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Events;

public class OrderPlacedEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public string OrderNumber { get; }
    public Money TotalAmount { get; }
    public int TotalQuantity { get; }
    public DateTime OccurredOn { get; }
    public Guid EventId { get; }

    public OrderPlacedEvent(Guid orderId, Guid userId, string orderNumber, Money totalAmount, int totalQuantity)
    {
        OrderId = orderId;
        UserId = userId;
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
        TotalQuantity = totalQuantity;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}