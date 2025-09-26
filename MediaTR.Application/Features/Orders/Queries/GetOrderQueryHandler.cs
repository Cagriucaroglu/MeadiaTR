using MediatR;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Domain.Repositories;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, GetOrderResult?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<GetOrderResult?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return null;

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
    }
}