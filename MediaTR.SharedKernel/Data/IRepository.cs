using System.Linq.Expressions;

namespace MediaTR.SharedKernel.Data;

/// <summary>
/// Generic repository interface for entity data access operations.
/// Provides CRUD operations, querying, pagination, and transaction support.
/// All async operations support cancellation tokens for proper resource management.
/// </summary>
/// <typeparam name="TEntity">Entity type implementing BaseEntity</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity, new()
{
    /// <summary>Gets an entity by its ID</summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets all entities</summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets entities matching the specified predicate</summary>
    Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Adds a new entity</summary>
    Task AddAsync(TEntity entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity</summary>
    void Update(TEntity entity, Guid? correlationId = null, string? metadata = null);

    /// <summary>Updates an existing entity asynchronously</summary>
    Task UpdateAsync(TEntity entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity</summary>
    void Delete(TEntity entity, Guid? correlationId = null, string? metadata = null);

    /// <summary>Returns the total count of entities</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the count of entities matching the predicate</summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Checks if any entity matches the predicate</summary>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Gets entities with pagination support</summary>
    Task<IEnumerable<TEntity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Adds multiple entities in bulk</summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>Updates multiple entities in bulk</summary>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes multiple entities in bulk</summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the first entity matching the predicate or null</summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Gets a single entity matching the predicate or null (throws if multiple matches)</summary>
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Projects entities using the specified selector</summary>
    Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    /// <summary>Executes an action within a transaction</summary>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>Gets an entity with eager loading of related entities</summary>
    Task<TEntity?> GetWithIncludeAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
}
