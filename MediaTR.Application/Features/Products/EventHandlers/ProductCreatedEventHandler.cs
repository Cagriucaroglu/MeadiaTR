using MediatR;
using MediaTR.Domain.Events;
using Microsoft.Extensions.Logging;

namespace MediaTR.Application.Features.Products.EventHandlers;

public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        var product = notification.Payload;

        _logger.LogInformation("Product created: {ProductId} - {ProductName}",
            product.Id,
            product.Name);

        // TODO: Update search index
        // TODO: Send notification to subscribers
        // TODO: Update cache

        await Task.CompletedTask;
    }
}