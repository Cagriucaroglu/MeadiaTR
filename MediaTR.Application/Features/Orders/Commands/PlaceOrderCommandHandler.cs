using MediatR;
using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Errors;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Events;
using MediaTR.Domain.Events.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.Data;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;
using System.Text.Json;

namespace MediaTR.Application.Features.Orders.Commands;

public class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly OrderBusinessLogic _orderBusinessLogic;
    private readonly OrderItemBusinessLogic _orderItemBusinessLogic;
    private readonly IMediator _mediator;
    private readonly IDbContext _dbContext;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        OrderBusinessLogic orderBusinessLogic,
        OrderItemBusinessLogic orderItemBusinessLogic,
        IMediator mediator,
        IDbContext dbContext)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderBusinessLogic = orderBusinessLogic;
        _orderItemBusinessLogic = orderItemBusinessLogic;
        _mediator = mediator;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcı kontrolü
        User? user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<Guid>(OrderErrors.UserNotFound(request.UserId));

        // Sipariş oluştur
        Order order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Notes = request.Notes,
            PaymentMethod = request.PaymentMethod,
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

        foreach (var itemDto in request.OrderItems)
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

        // Outbox Event oluştur (Eventual Consistency Pattern)
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
            Status = OutboxStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = request.CorrelationId,
            ConsistencyLevel = ConsistencyLevel.Eventual
        };

        // OutboxEvent'i aynı transaction içinde kaydet (Atomik!)
        var outboxRepository = _dbContext.GetRepository<OutboxEvent>();
        await outboxRepository.AddAsync(outboxEvent);

        // Domain event publish edilecek (OrderBusinessLogic içinde order.Raise() çağrıldı)
        // Outbox event arka planda OutboxProcessor tarafından işlenecek

        return Result.Success(order.Id);
    }
}