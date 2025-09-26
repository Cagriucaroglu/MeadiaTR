using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Events;

public class AdvertisementPublishedEvent : IDomainEvent
{
    public Guid AdvertisementId { get; }
    public Guid ProductId { get; }
    public Guid SellerId { get; }
    public Money Price { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }
    public Guid EventId { get; }

    public AdvertisementPublishedEvent(Guid advertisementId, Guid productId, Guid sellerId, Money price, string title)
    {
        AdvertisementId = advertisementId;
        ProductId = productId;
        SellerId = sellerId;
        Price = price;
        Title = title;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}