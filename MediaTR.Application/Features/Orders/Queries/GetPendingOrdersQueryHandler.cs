using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetPendingOrdersQueryHandler : IQueryHandler<GetPendingOrdersQuery, List<GetOrderResult>>
{
    private readonly IOrderRepository _orderRepository;

    public GetPendingOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<List<GetOrderResult>>> Handle(GetPendingOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByStatusAsync(OrderStatus.Pending, cancellationToken);

        var result = orders.Select(order =>
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

        return result; // Implicit operator: List<GetOrderResult> → Result<List<GetOrderResult>>
    }
}