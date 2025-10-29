using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Errors;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Events;
using MediaTR.Domain.Events.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using System.Text.Json;

namespace MediaTR.Application.Features.Orders.Commands;

/// <summary>
/// PlaceOrderCommand handler with transactional support and fire-and-wait outbox pattern
/// </summary>
internal sealed class PlaceOrderCommandHandler : TransactionalCommandHandlerBase<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly OrderBusinessLogic _orderBusinessLogic;
    private readonly OrderItemBusinessLogic _orderItemBusinessLogic;

    public PlaceOrderCommandHandler(
        IServiceProvider serviceProvider,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        OrderBusinessLogic orderBusinessLogic,
        OrderItemBusinessLogic orderItemBusinessLogic)
        : base(serviceProvider)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderBusinessLogic = orderBusinessLogic;
        _orderItemBusinessLogic = orderItemBusinessLogic;
    }

    protected override async Task<Result<Guid>> ProcessCommandAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcı kontrolü
        User? user = await _userRepository.GetByIdAsync(request.Request.UserId);
        if (user == null)
            return Result.Failure<Guid>(OrderErrors.UserNotFound(request.Request.UserId));

        // Sipariş oluştur
        Order order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.Request.UserId,
            ShippingAddress = request.Request.ShippingAddress,
            BillingAddress = request.Request.BillingAddress,
            Notes = request.Request.Notes,
            PaymentMethod = request.Request.PaymentMethod,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            TotalAmount = Money.Zero(),
            ShippingCost = Money.Zero(),
            TaxAmount = Money.Zero(),
            DiscountAmount = Money.Zero()
        };

        // OrderItem'ları ekle ve toplam tutarı hesapla
        decimal totalAmount = 0;
        string currency = "USD";

        foreach (var itemDto in request.Request.OrderItems)
        {
            var orderItem = await _orderItemBusinessLogic.CreateOrderItem(order.Id, itemDto.ProductId, itemDto.Quantity);
            _orderBusinessLogic.AddOrderItem(order, orderItem);
            totalAmount += orderItem.TotalPrice.Amount;
            currency = orderItem.UnitPrice.Currency;
        }

        order.TotalAmount = Money.Create(totalAmount, currency);

        // Business logic validation and processing
        await _orderBusinessLogic.PlaceOrder(order, request.CorrelationId);

        // Repository'ye kaydet
        await _orderRepository.AddAsync(order);

        // Outbox Event oluştur (Fire-and-Wait Pattern)
        var orderPlacedEvent = new OrderPlacedEvent
        {
            Payload = order,
            CorrelationId = request.CorrelationId
        };

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "OrderPlaced",
            AggregateId = order.Id,
            AggregateType = nameof(Order),
            Payload = JsonSerializer.Serialize(orderPlacedEvent),
            Status = OutboxStatus.Immediate,  // 🔥 Fire-and-wait mode
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = request.CorrelationId,
            ConsistencyLevel = ConsistencyLevel.Strong  // Strong consistency for immediate processing
        };

        // OutboxEvent'i BusinessLogicContext'e track et
        request.BusinessLogicContext.TrackOutboxEvent(outboxEvent);

        // OutboxEvent'i database'e ekle (transaction içinde)
        var outboxRepository = Context.GetRepository<OutboxEvent>();
        await outboxRepository.AddAsync(outboxEvent);

        return Result.Success(order.Id);
    }

    /// <summary>
    /// 🔥 KRITIK: Transaction commit'ten ÖNCE event'leri işle (Fire-and-Wait)
    /// </summary>
    protected override async Task BeforeTransactionCommittedAsync(
        Result<Guid> result,
        BusinessLogicContext blContext,
        CancellationToken cancellationToken)
    {
        if (result.IsSuccess)
        {
            // Fire-and-wait: Event'leri hemen işlemeyi dene
            // Başarısızsa Pending'e düşecek ve background worker işleyecek
            await ProcessOutboxEventsAsync(blContext, cancellationToken);
        }
    }
}