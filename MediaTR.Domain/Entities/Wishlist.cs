using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

/// <summary>
/// Wishlist entity (MongoDB persistence + Redis cache)
/// Stores user's favorite products for quick access
/// </summary>
public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    private readonly List<Guid> _productIds = new();
    public IReadOnlyCollection<Guid> ProductIds => _productIds.AsReadOnly();

    /// <summary>
    /// Add product to wishlist (idempotent)
    /// </summary>
    public void AddProduct(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (!_productIds.Contains(productId))
        {
            _productIds.Add(productId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Remove product from wishlist
    /// </summary>
    public void RemoveProduct(Guid productId)
    {
        if (_productIds.Remove(productId))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Check if product is in wishlist
    /// </summary>
    public bool Contains(Guid productId) => _productIds.Contains(productId);

    /// <summary>
    /// Total number of products in wishlist
    /// </summary>
    public int Count => _productIds.Count;

    /// <summary>
    /// Clear all products from wishlist
    /// </summary>
    public void Clear()
    {
        if (_productIds.Count > 0)
        {
            _productIds.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
