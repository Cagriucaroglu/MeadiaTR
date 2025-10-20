using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using MediaTR.Infrastructure.Data;
using MediaTR.Infrastructure.Repositories;
using MediaTR.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaTR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // SQL Server DbContext for Outbox Pattern (Aspire provides connection string)
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("MediaTROutbox");
            options.UseSqlServer(connectionString);
        });

        // Register IDbContext
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // MongoDB Configuration - Aspire will provide the connection string
        services.Configure<MongoDbSettings>(options =>
        {
            // Aspire provides connection string via MediaTRDb connection name
            options.ConnectionString = configuration.GetConnectionString("MediaTRDb");
            options.DatabaseName = "MediaTR";
        });

        // Repository registrations (MongoDB repositories)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}