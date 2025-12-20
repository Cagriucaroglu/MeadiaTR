using MediaTR.Domain.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediaTR.Infrastructure.Repositories;

/// <summary>
/// Wishlist repository implementation using MongoDB
/// </summary>
public class WishlistRepository : MongoRepository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
        // Create index on UserId for fast lookup
        CreateIndexes();
    }

    public async Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Wishlist>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Wishlist>.Filter.Eq(x => x.UserId, userId);
        var count = await _collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 }, cancellationToken);
        return count > 0;
    }

    private void CreateIndexes()
    {
        // Ensure index on UserId for fast user-based queries
        var indexKeys = Builders<Wishlist>.IndexKeys.Ascending(x => x.UserId);
        var indexModel = new CreateIndexModel<Wishlist>(indexKeys, new CreateIndexOptions { Unique = true });

        try
        {
            _collection.Indexes.CreateOne(indexModel);
        }
        catch
        {
            // Index might already exist, ignore error
        }
    }
}
