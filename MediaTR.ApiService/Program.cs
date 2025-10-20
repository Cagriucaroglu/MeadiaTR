
using MediaTR.ApiService.Extensions;
using MediaTR.Application;
using MediaTR.Infrastructure;
using Serilog;

namespace MediaTR.ApiService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog
            builder.Host.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            // Add service defaults
            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddProblemDetails(); // RFC 7807 standard

            // Add Application layer (MediatR, OutboxProcessor, etc.)
            builder.Services.AddApplication(builder.Configuration);

            // Add Infrastructure layer (Repositories, MongoDB, etc.)
            builder.Services.AddInfrastructure(builder.Configuration);

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapDefaultEndpoints();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // Built-in exception handling (OptimatePlatform approach)
            app.UseExceptionHandler();

            // Custom CorrelationId tracking middleware
            app.UseRequestContextLogging();

            // Built-in Serilog request logging
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            // TODO: Add Authentication & Authorization when implementing JWT
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
