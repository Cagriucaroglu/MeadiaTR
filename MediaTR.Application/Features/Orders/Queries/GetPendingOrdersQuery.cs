using MediatR;
using MediaTR.Application.Features.Orders.DTOs;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetPendingOrdersQuery : IRequest<List<GetOrderResult>>
{
    // Admin/Manager'ların pending siparişleri görmek için kullanacağı query
}