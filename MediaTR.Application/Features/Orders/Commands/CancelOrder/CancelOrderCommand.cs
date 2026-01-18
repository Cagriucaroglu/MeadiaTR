using MediaTR.Application.Abstractions.Messaging;

namespace MediaTR.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Cancel order command
/// User can cancel their own pending/confirmed orders
/// </summary>
public record CancelOrderCommand(Guid OrderId) : ICommand<bool>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
