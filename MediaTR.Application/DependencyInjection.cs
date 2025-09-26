using System.Reflection;
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