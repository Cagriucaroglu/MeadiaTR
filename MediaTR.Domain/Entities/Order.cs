using MediaTR.Domain.Enums;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; } 
    public User? User { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public Money TotalAmount { get; set; } 
    public Money ShippingCost { get; set; } 
    public Money TaxAmount { get; set; } 
    public Money DiscountAmount { get; set; } 
    public Address ShippingAddress { get; set; }
    public Address BillingAddress { get; set; } 
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentTransactionId { get; set; }

    public List<OrderItem> OrderItems { get; set; } = [];

    public Money SubtotalAmount => Money.FromDecimal(
        OrderItems.Sum(item => item.UnitPrice.Amount * item.Quantity),
        TotalAmount.Currency
    );

    public int TotalQuantity => OrderItems.Sum(item => item.Quantity);

    public bool CanBeCancelled => Status is OrderStatus.Pending or OrderStatus.Confirmed;
    public bool CanBeShipped => Status == OrderStatus.Processing;
    public bool IsCompleted => Status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Returned or OrderStatus.Refunded;

}