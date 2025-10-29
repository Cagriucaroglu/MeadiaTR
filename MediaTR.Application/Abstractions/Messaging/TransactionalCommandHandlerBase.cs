using Microsoft.Extensions.DependencyInjection;
using MediaTR.Application.Extensions;
using MediaTR.Application.Services.OutboxProcessor;
using MediaTR.Domain.Errors;
using MediaTR.Domain.Events.Entities;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.Data;
using MediaTR.SharedKernel.Outbox;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Abstractions.Messaging;

/// <summary>
/// Base class for transactional command handlers with automatic Outbox pattern support
/// </summary>
public abstract class TransactionalCommandHandlerBase<TCommand, TResponse>
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>, ICommandWrapper
{
    protected IDbContext Context { get; }
    private readonly IServiceProvider _serviceProvider;

    protected TransactionalCommandHandlerBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Context = serviceProvider.GetRequiredService<IDbContext>();
    }

    /// <summary>
    /// MediatR Handle metodu - Transaction scope içinde çalışır
    /// </summary>
    public async Task<Result<TResponse>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        TransactionResult<Result<TResponse>> scope = await Context.TransactionScopeAsync(
            ProcessCommandAsync,
            request,
            request.BusinessLogicContext,
            cancellationToken,
            afterTransactionStartCallback: AfterTransactionStartedAsync,
            beforeTransactionCommitCallback: BeforeTransactionCommittedAsync
        ).ConfigureAwait(false);

        return scope.Exception is not null
            ? throw scope.Exception
            : scope.Result!;
    }

    /// <summary>
    /// Override edilmesi gereken asıl business logic metodu
    /// </summary>
    protected abstract Task<Result<TResponse>> ProcessCommandAsync(
        TCommand request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Transaction başladıktan sonra çağrılır (optional)
    /// </summary>
    protected virtual Task AfterTransactionStartedAsync(
        BusinessLogicContext blContext,
        CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Transaction commit'ten önce çağrılır (optional)
    /// </summary>
    protected virtual Task BeforeTransactionCommittedAsync(
        Result<TResponse> result,
        BusinessLogicContext blContext,
        CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Processes tracked OutboxEvents in fire-and-wait mode after transaction commit
    /// </summary>
    protected async Task<OutboxExecutionResult> ProcessOutboxEventsAsync(
        BusinessLogicContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        IReadOnlyList<IOutboxEvent> trackedEvents = context.GetTrackedOutboxEvents();

        if (trackedEvents.Count == 0)
            return new OutboxExecutionResult();

        // 1. OutboxEvent'leri database'e kaydet
        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 2. Transaction'ı commit et (atomicity garantisi)
        if (context.Transaction != null)
        {
            await context.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        // 3. OutboxProcessor'ı al ve event'leri hemen işle
        OutboxProcessor? processor = _serviceProvider.GetService<OutboxProcessor>();

        if (processor == null)
        {
            // Processor yoksa - event'leri Pending yapıp background'a bırak
            OutboxExecutionResult fallbackResult = new();
            IRepository<OutboxEvent> outboxRepo = Context.GetRepository<OutboxEvent>();

            foreach (IOutboxEvent evt in trackedEvents)
            {
                OutboxEvent? outboxEvent = await outboxRepo
                    .FirstOrDefaultAsync(e => e.Id == evt.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (outboxEvent != null)
                {
                    outboxEvent.Status = OutboxStatus.Pending;
                    outboxEvent.LastError = "OutboxProcessor not available - deferred to background processing";
                    outboxRepo.Update(outboxEvent);
                }

                Error error = OutboxErrors.ProcessorNotFound();
                fallbackResult.AddResult(evt, Result.Failure(error));
            }

            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return fallbackResult;
        }

        // Event'leri hemen işle (fire-and-wait)
        OutboxExecutionResult result = await processor.ProcessImmediateAsync(
            Context,
            trackedEvents,
            cancellationToken).ConfigureAwait(false);

        // İşlenen event'leri temizle
        context.ClearTrackedOutboxEvents();

        return result;
    }
}
