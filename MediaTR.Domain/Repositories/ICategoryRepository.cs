using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.Data;

namespace MediaTR.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetCategoryHierarchyAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);
    Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}