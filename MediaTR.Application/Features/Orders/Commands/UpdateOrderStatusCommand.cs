using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.Enums;

namespace MediaTR.Application.Features.Orders.Commands;

/// <summary>
/// Update order status DTO
/// </summary>
public record UpdateOrderStatusDto(
    Guid OrderId,
    OrderStatus NewStatus,
    string? Notes = null,
    string? TrackingNumber = null);

/// <summary>
/// Update order status command using CommandWrapper pattern
/// </summary>
public sealed record UpdateOrderStatusCommand(
    UpdateOrderStatusDto Request,
    Guid CorrelationId)
    : CommandWrapper<UpdateOrderStatusDto, Guid>(Request, CorrelationId);