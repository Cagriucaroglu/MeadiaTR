using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediaTR.Infrastructure.Repositories;

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Email.Value, email);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(x => x.UserName, username);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Role, role);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(x => x.IsActive, true);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Email.Value, email);

        if (excludeUserId.HasValue && excludeUserId != Guid.Empty)
        {
            filter = Builders<User>.Filter.And(
                filter,
                Builders<User>.Filter.Ne(x => x.Id, excludeUserId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(x => x.UserName, username);

        if (excludeUserId.HasValue && excludeUserId != Guid.Empty)
        {
            filter = Builders<User>.Filter.And(
                filter,
                Builders<User>.Filter.Ne(x => x.Id, excludeUserId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }
}