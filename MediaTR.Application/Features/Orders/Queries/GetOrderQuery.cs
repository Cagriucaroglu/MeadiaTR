using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetOrderQuery : IQuery<GetOrderResult>
{
    public Guid OrderId { get; set; }

    public GetOrderQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}