using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using MediaTR.SharedKernel.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediaTR.Infrastructure.Repositories;

public class AdvertisementRepository : MongoRepository<Advertisement>, IAdvertisementRepository
{
    public AdvertisementRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }

    public async Task<IEnumerable<Advertisement>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Advertisement>> GetByStatusAsync(AdvertisementStatus status, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.Eq(x => x.Status, status);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Advertisement>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.Eq(x => x.CategoryId, categoryId);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Advertisement>> GetApprovedAdvertisementsAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.Eq(x => x.Status, AdvertisementStatus.Active);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Advertisement>> GetPendingAdvertisementsAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.Eq(x => x.Status, AdvertisementStatus.PendingApproval);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Advertisement>> GetExpiredAdvertisementsAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.Lt(x => x.ExpiresAt, DateTime.UtcNow);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Advertisement>> GetFeaturedAdvertisementsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.And(
            Builders<Advertisement>.Filter.Eq(x => x.IsFeatured, true),
            Builders<Advertisement>.Filter.Eq(x => x.Status, AdvertisementStatus.Active)
        );
        return await _collection.Find(filter).Limit(count).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Advertisement>> SearchAdvertisementsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Advertisement>.Filter.And(
            Builders<Advertisement>.Filter.Or(
                Builders<Advertisement>.Filter.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Advertisement>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            ),
            Builders<Advertisement>.Filter.Eq(x => x.Status, AdvertisementStatus.Active)
        );
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }
}