using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Events;

public class ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string Name { get; }
    public Money Price { get; }
    public Guid CategoryId { get; }
    public DateTime OccurredOn { get; }
    public Guid EventId { get; }

    public ProductCreatedEvent(Guid productId, string name, Money price, Guid categoryId)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        CategoryId = categoryId;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}