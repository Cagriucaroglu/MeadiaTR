using MediatR;
using MediaTR.Application.Features.Orders.DTOs;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetOrderQuery : IRequest<GetOrderResult?>
{
    public Guid OrderId { get; set; }

    public GetOrderQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}