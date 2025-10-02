using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;

namespace MediaTR.Application.Features.Orders.Queries;

public record GetUserOrdersQuery(Guid UserId) : IQuery<List<GetOrderResult>>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}