using MediatR;
using MediaTR.Application.Features.Orders.DTOs;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetUserOrdersQuery : IRequest<List<GetOrderResult>>
{
    public Guid UserId { get; set; }

    public GetUserOrdersQuery(Guid userId)
    {
        UserId = userId;
    }
}