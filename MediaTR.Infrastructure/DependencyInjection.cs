using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;
using MediaTR.Infrastructure.Configuration;
using MediaTR.Infrastructure.Data;
using MediaTR.Infrastructure.Repositories;
using MediaTR.Infrastructure.Services.Authentication;
using MediaTR.Infrastructure.Services.Cache;
using MediaTR.Infrastructure.Services.Cart;
using MediaTR.Infrastructure.Time;
using MediaTR.SharedKernel.Data;
using MediaTR.SharedKernel.Time;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MediaTR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DateTimeProvider - Singleton for time abstraction (testability)
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // SQL Server DbContext for Audit Trail (Outbox + RefreshTokens) - Aspire provides connection string
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("MediaTRAuditTrail");
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
        services.AddScoped<IWishlistRepository, WishlistRepository>();

        // JWT Settings Configuration - Bind appsettings.json JwtSettings section
        services.Configure<JwtSettings>(options =>
            configuration.GetSection(JwtSettings.SectionName).Bind(options));

        // Authentication Services
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Redis Cache - Aspire provides connection string via redis connection name
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("redis");
            options.InstanceName = "MediaTR:";
        });

        // Cache Services (uses IDistributedCache which is provided by StackExchangeRedisCache)
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

        // Shopping Cart Service (Redis-based)
        services.AddScoped<IShoppingCartService, ShoppingCartService>();

        // Data Protection - Store keys in Redis for multi-instance support
        var redisConnectionString = configuration.GetConnectionString("redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(redis, "MediaTR:DataProtection:Keys")
                .SetApplicationName("MediaTR");
        }

        return services;
    }
}