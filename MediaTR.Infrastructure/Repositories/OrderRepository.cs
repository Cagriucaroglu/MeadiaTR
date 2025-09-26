using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediaTR.Infrastructure.Repositories;

public class OrderRepository : MongoRepository<Order>, IOrderRepository
{
    public OrderRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter).SortByDescending(x => x.OrderDate).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Status, status);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.OrderNumber, orderNumber);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersInDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Gte(x => x.OrderDate, startDate),
            Builders<Order>.Filter.Lte(x => x.OrderDate, endDate)
        );
        return await _collection.Find(filter).SortByDescending(x => x.OrderDate).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Status, OrderStatus.Pending);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.OrderNumber, orderNumber);
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        string orderNumber;
        bool isUnique;

        do
        {
            var prefix = "ORD";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Random.Shared.Next(1000, 9999);
            orderNumber = $"{prefix}{timestamp}{random}";

            isUnique = await IsOrderNumberUniqueAsync(orderNumber, cancellationToken);
        }
        while (!isUnique);

        return orderNumber;
    }
}