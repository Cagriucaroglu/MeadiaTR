using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetPendingOrdersQuery : IQuery<List<GetOrderResult>>
{
    // Admin/Manager'ların pending siparişleri görmek için kullanacağı query
}