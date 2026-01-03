using System.Reflection;
using FluentValidation;
using MediatR;
using MediaTR.Application.Behaviors;
using MediaTR.Application.BusinessLogic;
using MediaTR.Application.Localization;
using MediaTR.Application.Services.OutboxProcessor;
using MediaTR.Application.Services.OutboxProcessor.OutboxHandlers;
using MediaTR.Domain.Services;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.Outbox;
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

            // Add pipeline behaviors (ORDER MATTERS!)
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));        // 1. Logging (first - logs everything)
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));     // 2. Validation (before execution)
        });

        // FluentValidation - Auto-register all validators from assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Localization - Scoped for per-request culture handling
        services.AddScoped<ILocalizationService, LocalizationService>();

        // Business Logic registrations
        services.AddScoped<ProductBusinessLogic>();
        services.AddScoped<CategoryBusinessLogic>();
        services.AddScoped<AdvertisementBusinessLogic>();
        services.AddScoped<OrderBusinessLogic>();
        services.AddScoped<OrderItemBusinessLogic>();
        services.AddScoped<ShoppingCartBusinessLogic>();
        services.AddScoped<WishlistBusinessLogic>();

        // Outbox Pattern - Background Service and Processor
        services.Configure<OutboxProcessorOptions>(cfg =>
            configuration.GetSection(OutboxProcessorOptions.SectionName));

        services.AddSingleton<OutboxProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessor>());

        // Outbox Event Handlers - Keyed Services (OptimatePlatform Pattern)
        services.AddKeyedScoped<IOutboxEventHandler, OrderPlacedEventHandler>("OrderPlaced");
        services.AddKeyedScoped<IOutboxEventHandler, ProductCreatedEventHandler>("ProductCreated");
        services.AddKeyedScoped<IOutboxEventHandler, AdvertisementPublishedEventHandler>("AdvertisementPublished");
        services.AddKeyedScoped<IOutboxEventHandler, OrderShippedEventHandler>("OrderShipped");
        services.AddKeyedScoped<IOutboxEventHandler, OrderDeliveredEventHandler>("OrderDelivered");
        services.AddKeyedScoped<IOutboxEventHandler, OrderCancelledEventHandler>("OrderCancelled");

        return services;
    }
}