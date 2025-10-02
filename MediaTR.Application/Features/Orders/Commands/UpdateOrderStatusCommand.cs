using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.Enums;

namespace MediaTR.Application.Features.Orders.Commands;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus,
    string? Notes = null,
    string? TrackingNumber = null
) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}