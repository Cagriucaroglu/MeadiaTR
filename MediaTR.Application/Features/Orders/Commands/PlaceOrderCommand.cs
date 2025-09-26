using MediatR;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Orders.Commands;

public class PlaceOrderCommand : IRequest<Guid>
{
    public Guid UserId { get; set; } 
    public Address ShippingAddress { get; set; }
    public Address BillingAddress { get; set; }
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
}
