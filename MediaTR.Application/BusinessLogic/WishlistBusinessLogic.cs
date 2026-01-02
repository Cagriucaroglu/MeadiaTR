using MediaTR.Domain.Entities;
using MediaTR.SharedKernel.Time;

namespace MediaTR.Application.BusinessLogic;

/// <summary>
/// Wishlist business logic
/// Handles wishlist entity creation, validation, and business rules
/// </summary>
public class WishlistBusinessLogic
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public WishlistBusinessLogic(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Create new wishlist for user
    /// </summary>
    public Wishlist CreateWishlist(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var wishlist = new Wishlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        return wishlist;
    }

    /// <summary>
    /// Add product to wishlist with business validation
    /// </summary>
    public void AddProduct(Wishlist wishlist, Product product)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        // Business validation
        if (!product.IsActive)
            throw new InvalidOperationException("Cannot add inactive product to wishlist");

        // Delegate to entity method (idempotent)
        wishlist.AddProduct(product.Id);

        // Entity method already updates UpdatedAt, but ensure we use IDateTimeProvider
        wishlist.UpdatedAt = _dateTimeProvider.UtcNow;
    }

    /// <summary>
    /// Remove product from wishlist
    /// </summary>
    public void RemoveProduct(Wishlist wishlist, Guid productId)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        // Delegate to entity method
        wishlist.RemoveProduct(productId);

        // Entity method already updates UpdatedAt, but ensure we use IDateTimeProvider
        wishlist.UpdatedAt = _dateTimeProvider.UtcNow;
    }

    /// <summary>
    /// Clear all products from wishlist
    /// </summary>
    public void ClearWishlist(Wishlist wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        // Delegate to entity method
        wishlist.Clear();

        // Entity method already updates UpdatedAt, but ensure we use IDateTimeProvider
        wishlist.UpdatedAt = _dateTimeProvider.UtcNow;
    }

    /// <summary>
    /// Check if product is in wishlist
    /// </summary>
    public bool Contains(Wishlist wishlist, Guid productId)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        return wishlist.Contains(productId);
    }

    /// <summary>
    /// Validate wishlist products (check if products still exist and are active)
    /// </summary>
    public IEnumerable<string> ValidateWishlist(Wishlist wishlist, IEnumerable<Product> products)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        var errors = new List<string>();
        var productDict = products.ToDictionary(p => p.Id);

        foreach (var productId in wishlist.ProductIds)
        {
            if (!productDict.TryGetValue(productId, out var product))
            {
                errors.Add($"Product {productId} not found");
                continue;
            }

            if (!product.IsActive)
            {
                errors.Add($"Product '{product.Name}' is no longer available");
            }
        }

        return errors;
    }
}
