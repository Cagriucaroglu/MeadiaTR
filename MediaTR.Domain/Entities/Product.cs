using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public Category? Category { get; set; }
    public Money Price { get; set; } = Money.Zero();
    public int StockQuantity { get; set; }
    public string Sku { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public double Weight { get; set; }
    public string? MainImageUrl { get; set; }

    private readonly List<ProductAttribute> _attributes = [];
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<string> _imageUrls = [];
    public IReadOnlyCollection<string> ImageUrls => _imageUrls.AsReadOnly();

    private readonly List<string> _tags = [];
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public bool IsInStock => StockQuantity > 0;
    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= 10;
}