using System.Reflection;
using MediatR;
using MediaTR.Application.Behaviors;
using MediaTR.Application.BusinessLogic;
using Microsoft.Extensions.DependencyInjection;

namespace MediaTR.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
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

        return services;
    }
}