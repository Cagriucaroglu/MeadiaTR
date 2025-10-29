using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Orders.Commands;

/// <summary>
/// Place order DTO
/// </summary>
public record PlaceOrderDto(
    Guid UserId,
    List<OrderItemRequest> OrderItems,
    Address ShippingAddress,
    Address BillingAddress,
    string? Notes = null,
    string? PaymentMethod = null);

/// <summary>
/// Place order command using CommandWrapper pattern
/// </summary>
public sealed record PlaceOrderCommand(
    PlaceOrderDto Request,
    Guid CorrelationId)
    : CommandWrapper<PlaceOrderDto, Guid>(Request, CorrelationId);

public record OrderItemRequest(
    Guid ProductId,
    int Quantity,
    Money UnitPrice);
