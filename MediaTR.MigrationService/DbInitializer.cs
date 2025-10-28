using MediaTR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaTR.MigrationService;

/// <summary>
/// OptimatePlatform standardında Migration Service
/// Database oluşturur, migration'ları uygular ve service'i kapatır
/// </summary>
public sealed class DbInitializer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<DbInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");

            using var scope = _serviceProvider.CreateScope();
            var dbContextFactory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

            await using var context = await dbContextFactory.CreateDbContextAsync(stoppingToken);

            _logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(stoppingToken);

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                    pendingMigrations.Count(),
                    string.Join(", ", pendingMigrations));

                await context.Database.MigrateAsync(stoppingToken);
                _logger.LogInformation("All migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("Database is up to date, no pending migrations");
            }

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
            throw;
        }
        finally
        {
            // Migration tamamlandı, service'i kapat (init container pattern)
            _logger.LogInformation("Stopping migration service...");
            _hostApplicationLifetime.StopApplication();
        }
    }
}
