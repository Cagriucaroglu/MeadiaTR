
using System.Text;
using MediaTR.ApiService.Extensions;
using MediaTR.Application;
using MediaTR.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

            // JWT Authentication Configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException(
                    "JWT SecretKey is not configured. Please set it in appsettings.json or user secrets.");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero // No tolerance for token expiration
                    };

                    // Optional: Add custom token validation events (Phase C - Token Blacklist)
                    // options.Events = new JwtBearerEvents
                    // {
                    //     OnTokenValidated = async context =>
                    //     {
                    //         // Check if token is blacklisted (Phase C)
                    //     }
                    // };
                });

            builder.Services.AddAuthorization();

            // Add Minimal API Endpoints (OptimatePlatform pattern)
            builder.Services.AddEndpoints(typeof(Program).Assembly);

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

            // Authentication & Authorization (JWT)
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Minimal API Endpoints (OptimatePlatform pattern)
            app.MapEndpoints();

            app.Run();
        }
    }
}
