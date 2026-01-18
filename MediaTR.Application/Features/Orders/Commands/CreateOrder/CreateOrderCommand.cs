using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Create order from shopping cart (Checkout)
/// Converts cart to order, validates stock, deducts inventory
/// </summary>
public record CreateOrderCommand(
    // Shipping Address
    string ShippingStreet,
    string ShippingCity,
    string ShippingState,
    string ShippingPostalCode,
    string ShippingCountry,

    // Billing Address (optional - if null, use shipping address)
    string? BillingStreet,
    string? BillingCity,
    string? BillingState,
    string? BillingPostalCode,
    string? BillingCountry,

    // Payment
    string PaymentMethod,  // "CreditCard", "PayPal", "CashOnDelivery"

    // Optional
    string? Notes) : ICommand<CreateOrderResult>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record CreateOrderResult(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount,
    string Currency,
    string Status);
