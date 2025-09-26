using MediaTR.Domain.Enums;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Orders.DTOs;

public record GetOrderResult(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    OrderStatus Status,
    DateTime OrderDate,
    DateTime? ShippedDate,
    DateTime? DeliveredDate,
    Money TotalAmount,
    Money ShippingCost,
    Money TaxAmount,
    Money DiscountAmount,
    Address ShippingAddress,
    Address BillingAddress,
    string? Notes,
    string? TrackingNumber,
    string? PaymentMethod,
    string? PaymentTransactionId,
    List<OrderItemDto> OrderItems,
    int TotalQuantity
);