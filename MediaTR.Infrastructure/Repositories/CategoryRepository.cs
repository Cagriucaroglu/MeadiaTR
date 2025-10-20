using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.Data;
using MediaTR.Domain.Repositories;
using MediaTR.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediaTR.Infrastructure.Repositories;

public class CategoryRepository : MongoRepository<Category>, ICategoryRepository
{
    private readonly IMongoCollection<Product> _productCollection;

    public CategoryRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
        _productCollection = _database.GetCollection<Product>("products");
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(x => x.Slug, slug);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.And(
            Builders<Category>.Filter.Or(
                Builders<Category>.Filter.Eq(x => x.ParentCategoryId, (Guid?)null),
                Builders<Category>.Filter.Eq(x => x.ParentCategoryId, Guid.Empty)
            ),
            Builders<Category>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.And(
            Builders<Category>.Filter.Eq(x => x.ParentCategoryId, parentId),
            Builders<Category>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var categories = new List<Category>();
        var currentCategory = await GetByIdAsync(categoryId, cancellationToken);

        while (currentCategory != null)
        {
            categories.Insert(0, currentCategory);
            if (currentCategory.ParentCategoryId == null || currentCategory.ParentCategoryId == Guid.Empty)
                break;
            currentCategory = await GetByIdAsync(currentCategory.ParentCategoryId.Value, cancellationToken);
        }

        return categories;
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(x => x.IsActive, true);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(x => x.Slug, slug);

        if (excludeCategoryId.HasValue && excludeCategoryId != Guid.Empty)
        {
            filter = Builders<Category>.Filter.And(
                filter,
                Builders<Category>.Filter.Ne(x => x.Id, excludeCategoryId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count == 0;
    }

    public async Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(x => x.ParentCategoryId, categoryId);
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId);
        var count = await _productCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }
}