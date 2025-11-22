using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Errors;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Events;
using MediaTR.Domain.Events.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using System.Text.Json;

namespace MediaTR.Application.Features.Orders.Commands;

/// <summary>
/// UpdateOrderStatusCommand handler with transactional support and fire-and-wait outbox pattern
/// Sends notifications for: Shipped, Delivered, Cancelled
/// </summary>
internal sealed class UpdateOrderStatusCommandHandler : TransactionalCommandHandlerBase<UpdateOrderStatusCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderBusinessLogic _orderBusinessLogic;

    public UpdateOrderStatusCommandHandler(
        IServiceProvider serviceProvider,
        IOrderRepository orderRepository,
        OrderBusinessLogic orderBusinessLogic)
        : base(serviceProvider)
    {
        _orderRepository = orderRepository;
        _orderBusinessLogic = orderBusinessLogic;
    }

    protected override async Task<Result<Guid>> ProcessCommandAsync(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        // Order'ı repository'den getir
        Order? order = await _orderRepository.GetByIdAsync(request.Request.OrderId, cancellationToken);
        if (order == null)
            return OrderErrors.NotFound;

        try
        {
            string? eventType = null;
            object? domainEvent = null;

            // Business logic'e göre durum değişikliğini yap
            switch (request.Request.NewStatus)
            {
                case OrderStatus.Confirmed:
                    _orderBusinessLogic.ConfirmOrder(order, request.CorrelationId);
                    break;
                case OrderStatus.Processing:
                    _orderBusinessLogic.ProcessOrder(order, request.CorrelationId);
                    break;
                case OrderStatus.Shipped:
                    if (string.IsNullOrWhiteSpace(request.Request.TrackingNumber))
                        return OrderErrors.TrackingNumberRequired;
                    _orderBusinessLogic.ShipOrder(order, request.Request.TrackingNumber, request.CorrelationId);
                    eventType = "OrderShipped";
                    domainEvent = new OrderShippedEvent { Payload = order, CorrelationId = request.CorrelationId };
                    break;
                case OrderStatus.Delivered:
                    _orderBusinessLogic.DeliverOrder(order, request.CorrelationId);
                    eventType = "OrderDelivered";
                    domainEvent = new OrderDeliveredEvent { Payload = order, CorrelationId = request.CorrelationId };
                    break;
                case OrderStatus.Cancelled:
                    _orderBusinessLogic.CancelOrder(order, request.CorrelationId);
                    eventType = "OrderCancelled";
                    domainEvent = new OrderCancelledEvent { Payload = order, CorrelationId = request.CorrelationId };
                    break;
                default:
                    return Error.Validation("Order.InvalidStatus", $"Status transition to {request.Request.NewStatus} is not supported");
            }

            // Notes varsa ekle
            if (!string.IsNullOrWhiteSpace(request.Request.Notes))
            {
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? request.Request.Notes
                    : $"{order.Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Request.Notes}";
            }

            // Repository'ye kaydet
            _orderRepository.Update(order);

            // Outbox Event oluştur (sadece Shipped, Delivered, Cancelled için)
            if (eventType != null && domainEvent != null)
            {
                var outboxEvent = new OutboxEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = eventType,
                    AggregateId = order.Id,
                    AggregateType = nameof(Order),
                    Payload = JsonSerializer.Serialize(domainEvent),
                    Status = OutboxStatus.Immediate,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CorrelationId = request.CorrelationId,
                    ConsistencyLevel = ConsistencyLevel.Strong
                };

                // OutboxEvent'i BusinessLogicContext'e track et
                request.BusinessLogicContext.TrackOutboxEvent(outboxEvent);

                // OutboxEvent'i database'e ekle (transaction içinde)
                var outboxRepository = Context.GetRepository<OutboxEvent>();
                await outboxRepository.AddAsync(outboxEvent);
            }

            return Result.Success(order.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation("Order.BusinessRuleViolation", ex.Message);
        }
    }

    /// <summary>
    /// Transaction commit'ten ÖNCE event'leri işle (Fire-and-Wait)
    /// </summary>
    protected override async Task BeforeTransactionCommittedAsync(
        Result<Guid> result,
        BusinessLogicContext blContext,
        CancellationToken cancellationToken)
    {
        if (result.IsSuccess)
        {
            await ProcessOutboxEventsAsync(blContext, cancellationToken);
        }
    }
}