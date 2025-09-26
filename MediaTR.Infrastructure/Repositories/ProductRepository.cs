using MediaTR.Domain.Entities;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediaTR.Infrastructure.Repositories;

public class ProductRepository : MongoRepository<Product>, IProductRepository
{
    public ProductRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Sku, sku);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Slug, slug);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Eq(x => x.IsFeatured, true),
            Builders<Product>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).Limit(count).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Gt(x => x.StockQuantity, 0),
            Builders<Product>.Filter.Lte(x => x.StockQuantity, 10),
            Builders<Product>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Product>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            ),
            Builders<Product>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.IsActive, true);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Sku, sku);

        if (excludeProductId.HasValue && excludeProductId != Guid.Empty)
        {
            filter = Builders<Product>.Filter.And(
                filter,
                Builders<Product>.Filter.Ne(x => x.Id, excludeProductId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Slug, slug);

        if (excludeProductId.HasValue && excludeProductId != Guid.Empty)
        {
            filter = Builders<Product>.Filter.And(
                filter,
                Builders<Product>.Filter.Ne(x => x.Id, excludeProductId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }
}