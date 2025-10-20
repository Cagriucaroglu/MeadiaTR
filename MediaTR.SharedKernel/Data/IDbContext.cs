namespace MediaTR.SharedKernel.Data;

/// <summary>
/// Database transaction interface for managing transaction lifecycle
/// </summary>
public interface IDbTransaction
{
    /// <summary>
    /// Indicates if the transaction is currently active
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Database context interface providing repository access, transaction management,
/// and query capabilities. Supports ExecutionStrategy-aware operations for resilient transactions.
/// </summary>
public interface IDbContext
{
    /// <summary>
    /// Gets a repository instance for the specified entity type
    /// </summary>
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new();

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes using bulk operations for better performance
    /// </summary>
    Task BulkSaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Executes an operation within a transaction using ExecutionStrategy for resilience
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<IDbTransaction, Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicates if the database connection is currently established
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Ensures the database connection is established
    /// </summary>
    Task<bool> EnsureConnectedAsync();

    /// <summary>
    /// Applies pending migrations to the database
    /// </summary>
    Task MigrateAsync();

    /// <summary>
    /// Gets a queryable for the specified entity type
    /// </summary>
    IQueryable<TEntity> Query<TEntity>() where TEntity : BaseEntity, new();
}
