using MediatR;
using MediaTR.Domain.Enums;

namespace MediaTR.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
}