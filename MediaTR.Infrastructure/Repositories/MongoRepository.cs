using System.Linq.Expressions;
using MediaTR.Infrastructure.Configuration;
using MediaTR.SharedKernel;
using MediaTR.SharedKernel.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediaTR.Infrastructure.Repositories;

public class MongoRepository<T> : IRepository<T> where T : BaseEntity, new()
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly IMongoDatabase _database;

    public MongoRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = _database.GetCollection<T>(GetCollectionName());
    }

    protected virtual string GetCollectionName()
    {
        return typeof(T).Name.ToLowerInvariant() + "s";
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    // New IRepository<T> interface methods
    public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public virtual void Update(T entity, Guid? correlationId = null, string? metadata = null)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        _collection.ReplaceOne(filter, entity);
    }

    public virtual async Task UpdateAsync(T entity, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }

    public virtual void Delete(T entity, Guid? correlationId = null, string? metadata = null)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        _collection.DeleteOne(filter);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return (int)await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return (int)await _collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(predicate, new CountOptions { Limit = 1 }, cancellationToken);
        return count > 0;
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var entitiesList = entities.ToList();
        foreach (var entity in entitiesList)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        await _collection.InsertManyAsync(entitiesList, cancellationToken: cancellationToken);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, Guid? correlationId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var ids = entities.Select(e => e.Id).ToList();
        var filter = Builders<T>.Filter.In(x => x.Id, ids);
        await _collection.DeleteManyAsync(filter, cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(predicate).SingleOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
    {
        var projection = Builders<T>.Projection.Expression(selector);
        return await _collection.Find(_ => true).Project(projection).ToListAsync(cancellationToken);
    }

    public virtual async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        // MongoDB transactions require replica sets
        // For simplicity, just execute the action without transaction
        await action();
    }

    public virtual async Task<T?> GetWithIncludeAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        // MongoDB doesn't support includes like EF Core
        // Just return the document without population
        return await _collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
    }
}