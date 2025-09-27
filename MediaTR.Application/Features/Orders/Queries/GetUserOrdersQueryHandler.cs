using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Orders.DTOs;
using MediaTR.Application.Orders.Errors;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Orders.Queries;

public class GetUserOrdersQueryHandler : IQueryHandler<GetUserOrdersQuery, List<GetOrderResult>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public GetUserOrdersQueryHandler(IOrderRepository orderRepository, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<GetOrderResult>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return OrderErrors.UserNotFound(request.UserId);

        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

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