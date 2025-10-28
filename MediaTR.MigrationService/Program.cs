using MediaTR.Infrastructure.Data;
using MediaTR.MigrationService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Aspire Service Defaults (Health checks, Telemetry, etc.)
builder.AddServiceDefaults();

// DbContextFactory with retry on failure policy
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MediaTROutbox");

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("MediaTR.Infrastructure");
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });

    // Development'ta detaylı logging
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Migration worker service
builder.Services.AddHostedService<DbInitializer>();

var host = builder.Build();
host.Run();
