#pragma warning disable SA1117, S138, CA1848, CA1031, S2221

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediaTR.Domain.Events.Entities;
using MediaTR.Domain.Errors;
using MediaTR.SharedKernel.Data;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Services.OutboxProcessor;

/// <summary>
/// Background service that processes outbox events for eventual consistency.
/// Polls the OutboxEvents table and dispatches events to registered handlers.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly TimeSpan _errorRetryDelay = TimeSpan.FromSeconds(30);

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        IOptions<OutboxProcessorOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
        _pollingInterval = TimeSpan.FromSeconds(_options.PollingIntervalSeconds);
    }

    /// <summary>
    /// Processes OutboxEvents immediately in fire-and-wait mode.
    /// Used by CommandHandlers after transaction commit.
    /// </summary>
    public async Task<Abstractions.Messaging.OutboxExecutionResult> ProcessImmediateAsync(
        IDbContext dbContext,
        IEnumerable<IOutboxEvent> events,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(events);

        Abstractions.Messaging.OutboxExecutionResult result = new();
        IRepository<OutboxEvent> outboxRepo = dbContext.GetRepository<OutboxEvent>();

        foreach (IOutboxEvent trackedEvent in events)
        {
            try
            {
                _logger.LogInformation(
                    "Processing immediate outbox event {EventId} of type '{EventType}'",
                    trackedEvent.Id,
                    trackedEvent.EventType);

                // Create a scope for resolving scoped services
                using IServiceScope scope = _serviceProvider.CreateScope();
                IOutboxEventHandler? handler = scope.ServiceProvider
                    .GetKeyedService<IOutboxEventHandler>(trackedEvent.EventType);

                if (handler == null)
                {
                    _logger.LogWarning(
                        "No handler found for event type '{EventType}'",
                        trackedEvent.EventType);

                    Error error = OutboxErrors.HandlerNotFound(trackedEvent.EventType);
                    result.AddResult(trackedEvent, Result.Failure(error));

                    // Update status to Failed
                    await UpdateOutboxStatusInDbAsync(
                        outboxRepo,
                        trackedEvent.Id,
                        OutboxStatus.Failed,
                        error.Description,
                        cancellationToken);
                    continue;
                }

                // Process the event
                Result processResult = await handler.ProcessAsync(trackedEvent, cancellationToken);
                result.AddResult(trackedEvent, processResult);

                if (processResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Successfully processed immediate outbox event {EventId}",
                        trackedEvent.Id);

                    // Update status to Processed
                    await UpdateOutboxStatusInDbAsync(
                        outboxRepo,
                        trackedEvent.Id,
                        OutboxStatus.Processed,
                        null,
                        cancellationToken);
                }
                else
                {
                    string errorMessage = processResult.Error?.Description ?? "Unknown processing error";

                    _logger.LogWarning(
                        "Immediate outbox event {EventId} failed, falling back to background processing. Error: {Error}",
                        trackedEvent.Id,
                        errorMessage);

                    // Failed - change to Pending for background processing
                    await UpdateOutboxStatusInDbAsync(
                        outboxRepo,
                        trackedEvent.Id,
                        OutboxStatus.Pending,
                        errorMessage,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception processing immediate outbox event {EventId}",
                    trackedEvent.Id);

                Error error = OutboxErrors.ProcessingFailed(trackedEvent.EventType, ex.Message);
                result.AddResult(trackedEvent, Result.Failure(error));

                // Exception - change to Pending for background processing
                await UpdateOutboxStatusInDbAsync(
                    outboxRepo,
                    trackedEvent.Id,
                    OutboxStatus.Pending,
                    ex.Message,
                    cancellationToken);
            }
        }

        // Save all status updates
        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    private async Task UpdateOutboxStatusInDbAsync(
        IRepository<OutboxEvent> repository,
        Guid eventId,
        OutboxStatus newStatus,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        OutboxEvent? outboxEvent = await repository.FirstOrDefaultAsync(
            e => e.Id == eventId,
            cancellationToken);

        if (outboxEvent != null)
        {
            outboxEvent.Status = newStatus;
            outboxEvent.LastError = errorMessage;

            if (newStatus == OutboxStatus.Processed)
            {
                outboxEvent.ProcessedAt = DateTimeOffset.UtcNow;
            }
            else if (newStatus == OutboxStatus.Pending)
            {
                outboxEvent.RetryCount++;
            }

            repository.Update(outboxEvent);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxProcessor started. Polling interval: {Interval}s",
            _pollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in OutboxProcessor. Retrying in {Delay}s",
                    _errorRetryDelay.TotalSeconds);

                try
                {
                    await Task.Delay(_errorRetryDelay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IDbContext dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

        try
        {
            // Get repository for OutboxEvent
            IRepository<OutboxEvent> outboxRepository = dbContext.GetRepository<OutboxEvent>();

            // First, check for stale Immediate events and convert them to Pending
            await ConvertStaleImmediateEventsToPendingAsync(outboxRepository, dbContext, cancellationToken);

            // Get pending events (skip Immediate status - those are handled by fire-and-wait)
            IEnumerable<OutboxEvent> allPending = await outboxRepository
                .GetAsync(e => e.Status == OutboxStatus.Pending && e.RetryCount < e.MaxRetries, cancellationToken);

            List<OutboxEvent> pendingEvents = allPending
                .OrderBy(e => e.CreatedAt)
                .Take(_options.BatchSize)
                .ToList();

            if (pendingEvents.Count == 0)
            {
                return;
            }

            _logger.LogDebug("Processing {Count} pending outbox event(s)", pendingEvents.Count);

            // Process each event
            foreach (OutboxEvent outboxEvent in pendingEvents)
            {
                await ProcessSingleEventAsync(scope, outboxRepository, outboxEvent, cancellationToken);
            }

            // Save all changes
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process pending outbox events");
        }
    }

    private async Task ProcessSingleEventAsync(
        IServiceScope scope,
        IRepository<OutboxEvent> repository,
        OutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing outbox event {EventId} of type '{EventType}'",
                outboxEvent.Id,
                outboxEvent.EventType);

            // Resolve handler from DI using keyed service
            IOutboxEventHandler? handler = scope.ServiceProvider
                .GetKeyedService<IOutboxEventHandler>(outboxEvent.EventType);

            if (handler == null)
            {
                _logger.LogWarning(
                    "No handler found for event type '{EventType}'",
                    outboxEvent.EventType);

                outboxEvent.Status = OutboxStatus.Failed;
                outboxEvent.LastError = OutboxErrors.HandlerNotFound(outboxEvent.EventType).Description;
                outboxEvent.ProcessedAt = DateTimeOffset.UtcNow;
                repository.Update(outboxEvent);
                return;
            }

            // Process the event
            Result result = await handler.ProcessAsync(outboxEvent, cancellationToken);

            if (result.IsSuccess)
            {
                outboxEvent.Status = OutboxStatus.Processed;
                outboxEvent.ProcessedAt = DateTimeOffset.UtcNow;
                outboxEvent.LastError = null;
                repository.Update(outboxEvent);

                _logger.LogInformation(
                    "Successfully processed outbox event {EventId}",
                    outboxEvent.Id);
            }
            else
            {
                HandleProcessingFailure(repository, outboxEvent, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception processing outbox event {EventId}",
                outboxEvent.Id);

            outboxEvent.RetryCount++;
            outboxEvent.LastError = ex.Message;

            if (outboxEvent.RetryCount >= outboxEvent.MaxRetries)
            {
                outboxEvent.Status = OutboxStatus.Failed;
                outboxEvent.ProcessedAt = DateTimeOffset.UtcNow;
            }

            repository.Update(outboxEvent);
        }
    }

    private void HandleProcessingFailure(
        IRepository<OutboxEvent> repository,
        OutboxEvent outboxEvent,
        Result result)
    {
        outboxEvent.RetryCount++;
        outboxEvent.LastError = result.Error?.Description ?? "Unknown processing error";

        if (outboxEvent.RetryCount >= outboxEvent.MaxRetries)
        {
            outboxEvent.Status = OutboxStatus.Failed;
            outboxEvent.ProcessedAt = DateTimeOffset.UtcNow;

            _logger.LogError(
                "Outbox event {EventId} failed after {RetryCount} retries. Error: {Error}",
                outboxEvent.Id,
                outboxEvent.RetryCount,
                outboxEvent.LastError);
        }
        else
        {
            _logger.LogWarning(
                "Outbox event {EventId} failed (attempt {RetryCount}/{MaxRetries}). Error: {Error}",
                outboxEvent.Id,
                outboxEvent.RetryCount,
                outboxEvent.MaxRetries,
                outboxEvent.LastError);
        }

        repository.Update(outboxEvent);
    }

    /// <summary>
    /// Converts stale Immediate events to Pending based on configured thresholds
    /// </summary>
    private async Task ConvertStaleImmediateEventsToPendingAsync(
        IRepository<OutboxEvent> outboxRepository,
        IDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Get all Immediate events
        IEnumerable<OutboxEvent> immediateEvents = await outboxRepository
            .GetAsync(e => e.Status == OutboxStatus.Immediate, cancellationToken);

        List<OutboxEvent> immediateEventsList = immediateEvents.ToList();

        if (immediateEventsList.Count == 0)
            return;

        DateTimeOffset staleThreshold = DateTimeOffset.UtcNow
            .AddSeconds(-_options.DefaultStaleThresholdSeconds);

        // Find stale events
        List<OutboxEvent> staleEvents = immediateEventsList
            .Where(e => e.CreatedAt < staleThreshold)
            .ToList();

        if (staleEvents.Count > 0)
        {
            _logger.LogWarning(
                "Found {Count} stale Immediate events (threshold: {Threshold}s)",
                staleEvents.Count,
                _options.DefaultStaleThresholdSeconds);

            foreach (OutboxEvent staleEvent in staleEvents)
            {
                staleEvent.Status = OutboxStatus.Pending;
                staleEvent.LastError = $"Converted from stale Immediate status (threshold: {_options.DefaultStaleThresholdSeconds}s)";
            }

            await outboxRepository.UpdateRangeAsync(staleEvents, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Converted {Count} stale Immediate events to Pending status",
                staleEvents.Count);
        }
    }
}
