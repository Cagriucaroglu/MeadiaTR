using MediatR;
using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Errors;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderBusinessLogic _orderBusinessLogic;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        OrderBusinessLogic orderBusinessLogic)
    {
        _orderRepository = orderRepository;
        _orderBusinessLogic = orderBusinessLogic;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Result.Failure(OrderErrors.NotFound);

        try
        {
            // Business logic'e göre durum değişikliğini yap
            switch (request.NewStatus)
            {
                case OrderStatus.Confirmed:
                    _orderBusinessLogic.ConfirmOrder(order);
                    break;
                case OrderStatus.Processing:
                    _orderBusinessLogic.ProcessOrder(order);
                    break;
                case OrderStatus.Shipped:
                    if (string.IsNullOrWhiteSpace(request.TrackingNumber))
                        return Result.Failure(OrderErrors.TrackingNumberRequired);
                    _orderBusinessLogic.ShipOrder(order, request.TrackingNumber);
                    break;
                case OrderStatus.Delivered:
                    _orderBusinessLogic.DeliverOrder(order);
                    break;
                case OrderStatus.Cancelled:
                    _orderBusinessLogic.CancelOrder(order);
                    break;
                default:
                    return Result.Failure(Error.Validation("Order.InvalidStatus", $"Status transition to {request.NewStatus} is not supported"));
            }

            // Notes varsa ekle
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? request.Notes
                    : $"{order.Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Notes}";
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("Order.BusinessRuleViolation", ex.Message));
        }
    }
}