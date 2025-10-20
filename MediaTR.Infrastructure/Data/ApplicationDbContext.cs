using MediatR;
using MediaTR.Domain.Events.Entities;
using MediaTR.SharedKernel;
using MediaTR.SharedKernel.Data;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediaTR.Infrastructure.Data;

/// <summary>
/// Application database context implementing IDbContext.
/// Provides repository access, transaction management, and automatic domain event publishing.
/// </summary>
public class ApplicationDbContext : DbContext, IDbContext
{
    private readonly IPublisher _publisher;
    private readonly Dictionary<Type, object> _repositories = [];
    private IDbContextTransaction? _currentTransaction;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    // DbSets
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new()
    {
        if (!_repositories.ContainsKey(typeof(TEntity)))
        {
            Repository<TEntity> repository = new(this);
            _repositories.Add(typeof(TEntity), repository);
        }

        return (IRepository<TEntity>)_repositories[typeof(TEntity)];
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public async Task BulkSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // For now, just call SaveChangesAsync
        // In future, can use BulkExtensions library for true bulk operations
        await SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _currentTransaction ??= await Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    private sealed class TransactionAdapter : IDbTransaction
    {
        private readonly IDbContextTransaction _dbContextTransaction;

        public TransactionAdapter(IDbContextTransaction dbContextTransaction)
        {
            _dbContextTransaction = dbContextTransaction;
        }

        public bool IsActive { get; set; } = true;

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (IsActive)
                await _dbContextTransaction.CommitAsync(cancellationToken);
            IsActive = false;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (IsActive)
                await _dbContextTransaction.RollbackAsync(cancellationToken);
            IsActive = false;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<IDbTransaction, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        // Use EF Core's execution strategy to handle retries with transactions
        IExecutionStrategy strategy = Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using IDbContextTransaction transaction = await Database.BeginTransactionAsync(cancellationToken);
            TransactionAdapter adapter = new(transaction);
            try
            {
                T result = await operation(adapter);

                if (adapter.IsActive)
                    await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                if (adapter.IsActive)
                    await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public bool IsConnected => Database.CanConnect();

    public async Task<bool> EnsureConnectedAsync()
    {
        return await Database.CanConnectAsync();
    }

    public async Task MigrateAsync()
    {
        await Database.MigrateAsync();
    }

    public IQueryable<TEntity> Query<TEntity>() where TEntity : BaseEntity, new()
    {
        return Set<TEntity>().AsQueryable();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        int result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events after save (eventual consistency)
        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        List<IDomainEvent> domainEvents =
            ChangeTracker
                .Entries<BaseEntity>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    IReadOnlyCollection<IDomainEvent> events = entity.DomainEvents;
                    entity.ClearDomainEvents();
                    return events;
                })
                .ToList();

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent);
        }
    }
}
