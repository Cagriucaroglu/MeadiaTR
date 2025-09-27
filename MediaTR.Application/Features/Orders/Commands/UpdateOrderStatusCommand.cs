using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.Enums;

namespace MediaTR.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommand : ICommand
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
}