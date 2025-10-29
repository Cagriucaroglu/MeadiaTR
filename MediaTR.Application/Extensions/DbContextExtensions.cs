#pragma warning disable S2221, CA1031

using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.Data;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Extensions;

/// <summary>
/// Transaction exception wrapper
/// </summary>
public class TransactionException : Exception
{
    public TransactionException(string message) : base(message) { }
    public TransactionException(string message, Exception innerException) : base(message, innerException) { }
    public TransactionException() { }
    public Error? Error { get; init; }
}

/// <summary>
/// Transaction operation delegate
/// </summary>
internal delegate Task<TOut> TransactionalOperation<in TIn, TOut>(TIn args, CancellationToken cancellationToken);

/// <summary>
/// Transaction result wrapper
/// </summary>
public record TransactionResult<T>(T? Result, Exception? Exception);

/// <summary>
/// DbContext extension methods for transactional operations
/// </summary>
internal static class DbContextExtensions
{
    private static readonly Lock _lock = new();

    /// <summary>
    /// Executes operation within a transaction scope with callbacks
    /// </summary>
    internal static async Task<TransactionResult<TOut>> TransactionScopeAsync<TIn, TOut>(
        this IDbContext context,
        TransactionalOperation<TIn, TOut> func,
        TIn arg,
        BusinessLogicContext blContext,
        CancellationToken cancellationToken,
        Func<BusinessLogicContext, CancellationToken, Task>? afterTransactionStartCallback = null,
        Func<TOut, BusinessLogicContext, CancellationToken, Task>? beforeTransactionCommitCallback = null)
        where TOut : IResult
    {
        // Zaten transaction içindeysek, direkt çalıştır
        if (blContext.IsInTransaction)
        {
            try
            {
                TOut result = await func(arg, cancellationToken).ConfigureAwait(false);

                // Failure durumunda rollback
                if (result.IsFailure && blContext.Transaction is not null)
                    await blContext.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                return new TransactionResult<TOut>(result, null);
            }
            catch (Exception e)
            {
                if (blContext.Transaction is not null)
                    await blContext.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return new TransactionResult<TOut>(default, e);
            }
        }

        // Yeni transaction başlat
        return await context.ExecuteInTransactionAsync(async (transaction) =>
        {
            // Transaction context'i set et (thread-safe)
            lock (_lock)
            {
                blContext.IsInTransaction = true;
                blContext.Transaction = transaction;
            }

            try
            {
                // 1. Transaction başladıktan sonra callback
                if (afterTransactionStartCallback is not null)
                    await afterTransactionStartCallback(blContext, cancellationToken).ConfigureAwait(false);

                // 2. Asıl operation'ı çalıştır
                TOut result = await func(arg, cancellationToken).ConfigureAwait(false);

                // 3. Result kontrolü
                if (result.IsFailure)
                {
                    await blContext.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // 4. Commit öncesi callback
                    if (beforeTransactionCommitCallback is not null)
                        await beforeTransactionCommitCallback(result, blContext, cancellationToken).ConfigureAwait(false);

                    // 5. Transaction'ı commit et
                    await blContext.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }

                return new TransactionResult<TOut>(result, null);
            }
            catch (TransactionException e)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return new TransactionResult<TOut>(default, e);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return new TransactionResult<TOut>(default, e);
            }
            finally
            {
                // Context'i temizle
                lock (_lock)
                {
                    blContext.IsInTransaction = false;
                    blContext.Transaction = null;
                }
            }
        }, cancellationToken).ConfigureAwait(false);
    }
}
