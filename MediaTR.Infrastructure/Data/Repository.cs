using MediaTR.SharedKernel;
using MediaTR.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MediaTR.Infrastructure.Data;

/// <summary>
/// Generic repository implementation for entity data access operations.
/// Provides CRUD operations, querying, pagination, and transaction support.
/// </summary>
/// <typeparam name="TEntity">Entity type implementing BaseEntity</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity, new()
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        Guid? correlationId = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return;

        await _dbSet.AddAsync(entity, cancellationToken);
        // TODO: Add system event tracking in future
    }

    public void Update(TEntity entity, Guid? correlationId = null, string? metadata = null)
    {
        if (entity is null)
            return;

        _dbSet.Update(entity);
        // TODO: Add system event tracking in future
    }

    public async Task UpdateAsync(TEntity entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return;

        _dbSet.Update(entity);
        await Task.CompletedTask;
        // TODO: Add system event tracking in future
    }
    
    public void Delete(TEntity entity, Guid? correlationId = null, string? metadata = null)
    {
        if (entity is null)
            return;

        _dbSet.Remove(entity);
        // TODO: Add system event tracking in future
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        Guid? correlationId = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        // TODO: Add system event tracking in future
    }

    public async Task UpdateRangeAsync(
        IEnumerable<TEntity> entities,
        Guid? correlationId = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.UpdateRange(entities);
        await Task.CompletedTask;
        // TODO: Add system event tracking in future
    }

    public async Task DeleteRangeAsync(
        IEnumerable<TEntity> entities,
        Guid? correlationId = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
        await Task.CompletedTask;
        // TODO: Add system event tracking in future
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<TResult>> SelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return await _dbSet.Select(selector).ToListAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TEntity?> GetWithIncludeAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}
