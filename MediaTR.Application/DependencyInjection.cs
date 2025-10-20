using System.Reflection;
using MediatR;
using MediaTR.Application.Behaviors;
using MediaTR.Application.BusinessLogic;
using MediaTR.Application.Services.OutboxProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaTR.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Add pipeline behaviors (order matters - logging should be first)
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Business Logic registrations
        services.AddScoped<ProductBusinessLogic>();
        services.AddScoped<CategoryBusinessLogic>();
        services.AddScoped<AdvertisementBusinessLogic>();
        services.AddScoped<OrderBusinessLogic>();
        services.AddScoped<OrderItemBusinessLogic>();

        // Outbox Pattern - Background Service
        services.Configure<OutboxProcessorOptions>(options =>
            configuration.GetSection(OutboxProcessorOptions.SectionName));
        services.AddHostedService<OutboxProcessor>();

        // TODO: Register outbox event handlers here using keyed services
        // Example:
        // services.AddKeyedScoped<IOutboxEventHandler, OrderPlacedEventHandler>("OrderPlaced");

        return services;
    }
}