
using MediaTR.Application;
using MediaTR.Infrastructure;

namespace MediaTR.ApiService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add service defaults
            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddControllers();

            // Add Application layer (MediatR, etc.)
            builder.Services.AddApplication();

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

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
