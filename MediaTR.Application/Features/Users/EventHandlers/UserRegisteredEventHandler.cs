using MediatR;
using MediaTR.Domain.Events;
using Microsoft.Extensions.Logging;

namespace MediaTR.Application.Features.Users.EventHandlers;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var user = notification.Payload;

        _logger.LogInformation("User registered: {UserId} - {Email}",
            user.Id,
            user.Email.Value);

        // TODO: Send welcome email
        // TODO: Create user profile
        // TODO: Initialize user preferences

        await Task.CompletedTask;
    }
}