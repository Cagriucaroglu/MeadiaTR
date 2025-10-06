#pragma warning disable CA1848 // Use the LoggerMessage delegates

using MediatR;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;

namespace MediaTR.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior for structured logging with Result pattern support.
/// CorrelationId is automatically included from LogContext (set by RequestContextLoggingMiddleware).
/// </summary>
internal sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Processing request {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        TResponse result = await next();

        stopwatch.Stop();

        if (result.IsSuccess)
        {
            logger.LogInformation(
                "Completed request {RequestName} in {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Error, destructureObjects: true))
            {
                logger.LogError(
                    "Completed request {RequestName} with error in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        return result;
    }
}
