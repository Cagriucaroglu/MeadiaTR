using MediatR;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Repositories;

namespace MediaTR.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
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

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order with Id {request.OrderId} not found");

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
                    throw new ArgumentException("Tracking number is required for shipping");
                _orderBusinessLogic.ShipOrder(order, request.TrackingNumber);
                break;
            case OrderStatus.Delivered:
                _orderBusinessLogic.DeliverOrder(order);
                break;
            case OrderStatus.Cancelled:
                _orderBusinessLogic.CancelOrder(order);
                break;
            default:
                throw new ArgumentException($"Status transition to {request.NewStatus} is not supported");
        }

        // Notes varsa ekle
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                ? request.Notes
                : $"{order.Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Notes}";
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return true;
    }
}