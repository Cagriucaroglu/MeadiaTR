using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetUserOrdersQuery : IQuery<List<GetOrderResult>>
{
    public Guid UserId { get; set; }

    public GetUserOrdersQuery(Guid userId)
    {
        UserId = userId;
    }
}