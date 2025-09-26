using MediatR;
using MediaTR.Domain.Events;
using Microsoft.Extensions.Logging;

namespace MediaTR.Application.Features.Orders.EventHandlers;

public class OrderPlacedEventHandler : INotificationHandler<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;

    public OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order placed: OrderId={OrderId}, UserId={UserId}, OrderNumber={OrderNumber}, TotalAmount={TotalAmount}",
            notification.OrderId, notification.UserId, notification.OrderNumber, notification.TotalAmount);

        // Burada sipariş verildikten sonra yapılacak işlemler:
        // - Email gönderme
        // - SMS bildirimi
        // - Inventory güncelleme
        // - Analytics/reporting
        // - Third-party sistem entegrasyonları

        await Task.CompletedTask;
    }
}