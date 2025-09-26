using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public string? ImageUrl { get; set; }

    private readonly List<Category> _childCategories = [];
    public IReadOnlyCollection<Category> ChildCategories => _childCategories.AsReadOnly();

    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public bool IsRootCategory => ParentCategoryId is not null;
    public bool HasChildren => _childCategories.Any();
}