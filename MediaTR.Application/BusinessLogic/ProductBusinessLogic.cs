using MediaTR.Domain.Entities;
using MediaTR.Domain.Events;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Application.BusinessLogic;

public class ProductBusinessLogic
{
    public Product CreateProduct(string name, string description, Guid categoryId, Money price, string sku, int stockQuantity, Guid correlationId, double weight = 0)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

        // Create product entity
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CategoryId = categoryId,
            Price = price,
            Sku = sku,
            StockQuantity = stockQuantity,
            Weight = weight,
            Slug = GenerateSlug(name),
            IsActive = true,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Raise domain event
        product.Raise(new ProductCreatedEvent
        {
            Payload = product,
            CorrelationId = correlationId
        });

        return product;
    }

    public void UpdateStock(Product product, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public void ReduceStock(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (product.StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(Product product, bool isActive)
    {
        product.IsActive = isActive;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Product product, Money newPrice)
    {
        product.Price = newPrice;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(Product product, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        product.Name = name;
        product.Description = description;
        product.Slug = GenerateSlug(name);
        product.UpdatedAt = DateTime.UtcNow;
    }

    public void SetMainImage(Product product, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));

        product.MainImageUrl = imageUrl;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeatured(Product product, bool isFeatured)
    {
        product.IsFeatured = isFeatured;
        product.UpdatedAt = DateTime.UtcNow;
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