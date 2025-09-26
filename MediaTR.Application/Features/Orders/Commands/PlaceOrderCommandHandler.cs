using MediatR;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Events;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Orders.Commands;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly OrderBusinessLogic _orderBusinessLogic;
    private readonly OrderItemBusinessLogic _orderItemBusinessLogic;
    private readonly IMediator _mediator;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        OrderBusinessLogic orderBusinessLogic,
        OrderItemBusinessLogic orderItemBusinessLogic,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderBusinessLogic = orderBusinessLogic;
        _orderItemBusinessLogic = orderItemBusinessLogic;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcı kontrolü
        User? user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new InvalidOperationException($"User with Id {request.UserId} not found");

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
        await _orderBusinessLogic.PlaceOrder(order);

        // Repository'ye kaydet
        await _orderRepository.AddAsync(order);

        // Domain event publish edilecek (OrderBusinessLogic içinde order.Raise() çağrıldı)
        await _mediator.Publish(new OrderPlacedEvent(order.Id, order.UserId, order.OrderNumber, order.TotalAmount, order.TotalQuantity), cancellationToken);

        return order.Id;
    }
}