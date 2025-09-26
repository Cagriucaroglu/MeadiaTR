using MediatR;
using MediaTR.Domain.Events;
using Microsoft.Extensions.Logging;

namespace MediaTR.Application.Features.Advertisements.EventHandlers;

public class AdvertisementPublishedEventHandler : INotificationHandler<AdvertisementPublishedEvent>
{
    private readonly ILogger<AdvertisementPublishedEventHandler> _logger;

    public AdvertisementPublishedEventHandler(ILogger<AdvertisementPublishedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AdvertisementPublishedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Advertisement published: {AdvertisementId} - {Title}",
            notification.AdvertisementId,
            notification.Title);

        // TODO: Send notification to interested users
        // TODO: Update search index
        // TODO: Schedule expiration reminder

        await Task.CompletedTask;
    }
}