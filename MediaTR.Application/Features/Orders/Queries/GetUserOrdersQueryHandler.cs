using MediatR;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Domain.Repositories;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, List<GetOrderResult>>
{
    private readonly IOrderRepository _orderRepository;

    public GetUserOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<GetOrderResult>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return orders.Select(order =>
        {
            var orderItems = order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                Currency = item.UnitPrice.Currency
            }).ToList();

            return new GetOrderResult(
                order.Id,
                order.OrderNumber,
                order.UserId,
                order.Status,
                order.OrderDate,
                order.ShippedDate,
                order.DeliveredDate,
                order.TotalAmount,
                order.ShippingCost,
                order.TaxAmount,
                order.DiscountAmount,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.TrackingNumber,
                order.PaymentMethod,
                order.PaymentTransactionId,
                orderItems,
                order.TotalQuantity
            );
        }).ToList();
    }
}