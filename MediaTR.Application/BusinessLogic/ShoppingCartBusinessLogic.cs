using MediaTR.Domain.Entities;
using MediaTR.Domain.Entities.InMemory;
using MediaTR.SharedKernel.Time;

namespace MediaTR.Application.BusinessLogic;

/// <summary>
/// Shopping cart business logic
/// Handles cart entity creation, validation, and business rules
/// </summary>
public class ShoppingCartBusinessLogic
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ShoppingCartBusinessLogic(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Create new shopping cart for user or guest
    /// </summary>
    public ShoppingCart CreateCart(Guid userId)
    {
        return new ShoppingCart
        {
            UserId = userId, // Guid.Empty for guests
            Items = new List<ShoppingCartItem>(),
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };
    }

    /// <summary>
    /// Create cart item from product with business validation
    /// </summary>
    public ShoppingCartItem CreateCartItem(Product product, int quantity)
    {
        // Business validation
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (!product.IsActive)
            throw new InvalidOperationException("Cannot add inactive product to cart");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (product.StockQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {quantity}");

        return new ShoppingCartItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSku = product.Sku,
            UnitPrice = product.Price.Amount,
            Currency = product.Price.Currency,
            Quantity = quantity,
            ProductImageUrl = product.MainImageUrl,
            AddedAt = _dateTimeProvider.UtcNow
        };
    }

    /// <summary>
    /// Update cart item quantity with stock validation and price sync
    /// </summary>
    public void UpdateItemQuantity(ShoppingCartItem item, Product product, int newQuantity)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (!product.IsActive)
            throw new InvalidOperationException("Product is no longer active");

        if (newQuantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));

        if (product.StockQuantity < newQuantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {newQuantity}");

        item.Quantity = newQuantity;
        item.UnitPrice = product.Price.Amount; // Sync to latest price
    }

    /// <summary>
    /// Add or update item in cart
    /// </summary>
    public void AddOrUpdateItem(ShoppingCart cart, Product product, int quantity)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
        {
            // Update existing item
            var newQuantity = existingItem.Quantity + quantity;
            UpdateItemQuantity(existingItem, product, newQuantity);
        }
        else
        {
            // Add new item
            var newItem = CreateCartItem(product, quantity);
            cart.Items.Add(newItem);
        }

        cart.UpdatedAt = _dateTimeProvider.UtcNow;
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    public void RemoveItem(ShoppingCart cart, Guid productId)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.Items.Remove(item);
            cart.UpdatedAt = _dateTimeProvider.UtcNow;
        }
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    public void ClearCart(ShoppingCart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        if (cart.Items.Count > 0)
        {
            cart.Items.Clear();
            cart.UpdatedAt = _dateTimeProvider.UtcNow;
        }
    }

    /// <summary>
    /// Merge guest cart into user cart
    /// </summary>
    public void MergeGuestCart(ShoppingCart userCart, ShoppingCart guestCart, IEnumerable<Product> products)
    {
        if (userCart == null)
            throw new ArgumentNullException(nameof(userCart));

        if (guestCart == null || guestCart.Items.Count == 0)
            return; // Nothing to merge

        var productDict = products.ToDictionary(p => p.Id);

        foreach (var guestItem in guestCart.Items)
        {
            // Find product
            if (!productDict.TryGetValue(guestItem.ProductId, out var product))
                continue; // Product not found, skip

            if (!product.IsActive)
                continue; // Inactive product, skip

            var existingUserItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);

            if (existingUserItem != null)
            {
                // Merge quantities
                var mergedQuantity = existingUserItem.Quantity + guestItem.Quantity;

                // Cap at available stock
                if (mergedQuantity > product.StockQuantity)
                    mergedQuantity = product.StockQuantity;

                UpdateItemQuantity(existingUserItem, product, mergedQuantity);
            }
            else
            {
                // Add guest item to user cart (validate stock first)
                try
                {
                    var quantity = Math.Min(guestItem.Quantity, product.StockQuantity);
                    var newItem = CreateCartItem(product, quantity);
                    userCart.Items.Add(newItem);
                }
                catch
                {
                    // Skip items that can't be added
                    continue;
                }
            }
        }

        userCart.UpdatedAt = _dateTimeProvider.UtcNow;
    }

    /// <summary>
    /// Validate entire cart (check stock availability for all items)
    /// </summary>
    public IEnumerable<string> ValidateCart(ShoppingCart cart, IEnumerable<Product> products)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var errors = new List<string>();
        var productDict = products.ToDictionary(p => p.Id);

        foreach (var item in cart.Items)
        {
            if (!productDict.TryGetValue(item.ProductId, out var product))
            {
                errors.Add($"Product '{item.ProductName}' not found");
                continue;
            }

            if (!product.IsActive)
            {
                errors.Add($"Product '{item.ProductName}' is no longer available");
                continue;
            }

            if (product.StockQuantity < item.Quantity)
            {
                errors.Add($"Insufficient stock for '{item.ProductName}'. Available: {product.StockQuantity}, In cart: {item.Quantity}");
            }
        }

        return errors;
    }
}
