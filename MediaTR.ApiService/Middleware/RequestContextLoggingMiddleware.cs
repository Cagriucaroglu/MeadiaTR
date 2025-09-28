using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace MediaTR.ApiService.Middleware;

public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CORRELATION_ID_HEADER_NAME = "Correlation-Id";

    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            await next.Invoke(context).ConfigureAwait(false);
        }
    }

    private static string GetCorrelationId(HttpContext? context)
    {
        if (context is null)
            return Guid.NewGuid().ToString();

        context.Request.Headers.TryGetValue(
            CORRELATION_ID_HEADER_NAME,
            out StringValues correlationId);

        var finalCorrelationId = correlationId.FirstOrDefault() ?? context.TraceIdentifier;

        // Add CorrelationId to response headers for client tracking
        context.Response.Headers.TryAdd(CORRELATION_ID_HEADER_NAME, finalCorrelationId);

        return finalCorrelationId;
    }
}