using MediaTR.Domain.Entities;
using MediaTR.SharedKernel;

namespace MediaTR.Application.BusinessLogic;

public class CategoryBusinessLogic
{
    public Category CreateCategory(string name, string description, Guid? parentCategoryId = null, int sortOrder = 0)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description cannot be empty", nameof(description));

        // Create category entity
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Slug = GenerateSlug(name),
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return category;
    }

    public void UpdateInfo(Category category, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description cannot be empty", nameof(description));

        category.Name = name;
        category.Description = description;
        category.Slug = GenerateSlug(name);
        category.UpdatedAt = DateTime.UtcNow;
    }

    public void SetParentCategory(Category category, Guid? parentCategoryId)
    {
        // Business rule: Cannot set itself as parent
        if (parentCategoryId == category.Id)
            throw new InvalidOperationException("Category cannot be its own parent");

        category.ParentCategoryId = parentCategoryId;
        category.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSortOrder(Category category, int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentException("Sort order cannot be negative", nameof(sortOrder));

        category.SortOrder = sortOrder;
        category.UpdatedAt = DateTime.UtcNow;
    }

    public void SetImageUrl(Category category, string? imageUrl)
    {
        category.ImageUrl = imageUrl;
        category.UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(Category category)
    {
        category.IsActive = true;
        category.UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(Category category)
    {
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("ç", "c")
                   .Replace("ğ", "g")
                   .Replace("ı", "i")
                   .Replace("ö", "o")
                   .Replace("ş", "s")
                   .Replace("ü", "u");
    }
}