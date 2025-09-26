using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using MediaTR.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaTR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB Configuration - Aspire will provide the connection string
        services.Configure<MongoDbSettings>(options =>
        {
            // Aspire provides connection string via MediaTRDb connection name
            options.ConnectionString = configuration.GetConnectionString("MediaTRDb");
            options.DatabaseName = "MediaTR";
        });

        // Repository registrations
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}