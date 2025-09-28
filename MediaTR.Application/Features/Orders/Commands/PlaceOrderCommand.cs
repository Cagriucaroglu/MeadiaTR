using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Orders.Commands;

public record PlaceOrderCommand(
    Guid UserId,
    List<OrderItemRequest> OrderItems,
    Address ShippingAddress,
    Address BillingAddress,
    string? Notes = null,
    string? PaymentMethod = null
) : ICommand<Guid>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record OrderItemRequest(
    Guid ProductId,
    int Quantity,
    Money UnitPrice);
