using System.Reflection;
using MediaTR.ApiService.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediaTR.ApiService.Extensions;

internal static class EndpointExtensions
{
    internal static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        services.TryAddEnumerable([.. assembly
            .DefinedTypes
            .Where(static type => type is { IsAbstract: false, IsInterface: false } &&
                                  type.IsAssignableTo(typeof(IEndpoint)))
            .Select(static type => ServiceDescriptor.Transient(typeof(IEndpoint), type))]);

        return services;
    }

    internal static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder ?? (IEndpointRouteBuilder)app;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    internal static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder app, string permission)
    {
        return app.RequireAuthorization(permission);
    }
}
