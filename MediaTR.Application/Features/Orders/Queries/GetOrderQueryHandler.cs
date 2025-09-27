using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Application.Orders.Errors;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, GetOrderResult>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetOrderResult>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return OrderErrors.NotFound; // Implicit operator: Error → Result<GetOrderResult>

        var orderItems = order.OrderItems.Select(item => new OrderItemDto
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice.Amount,
            Currency = item.UnitPrice.Currency
        }).ToList();

        var result = new GetOrderResult(
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

        return result; // Implicit operator: GetOrderResult → Result<GetOrderResult>
    }
}