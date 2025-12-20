using MediaTR.Domain.Entities;
using MediaTR.Domain.Entities.InMemory;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;

namespace MediaTR.Infrastructure.Services.Cart;

/// <summary>
/// Shopping cart service using Redis for ultra-fast cart operations
/// Target: < 10ms operations for 1M+ users
/// </summary>
public class ShoppingCartService : IShoppingCartService
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;
    private const string CartKeyPrefix = "cart:user:";
    private const string GuestCartKeyPrefix = "cart:guest:";
    private static readonly TimeSpan CartExpiration = TimeSpan.FromDays(30);

    public ShoppingCartService(
        ICacheService cacheService,
        IProductRepository productRepository)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
    }

    #region User Cart Operations

    public async Task<ShoppingCart> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"{CartKeyPrefix}{userId}";
        var cart = await _cacheService.GetAsync<ShoppingCart>(key, cancellationToken);

        if (cart == null)
        {
            cart = new ShoppingCart { UserId = userId };
            await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
        }

        return cart;
    }

    public async Task AddItemAsync(Guid userId, Guid productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        // 1. Validate quantity
        if (quantity <= 0)
            throw new InvalidOperationException("Cart.InvalidQuantity");

        // 2. Get product from cache or DB
        var product = await GetProductWithCacheAsync(productId, cancellationToken);
        if (product == null || !product.IsActive)
            throw new InvalidOperationException("Product.NotFound");

        // 3. Check stock availability
        if (product.StockQuantity < quantity)
            throw new InvalidOperationException("Product.InsufficientStock");

        // 4. Get cart
        var cart = await GetCartAsync(userId, cancellationToken);

        // 5. Add or update item
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            // Update existing item
            var newQuantity = existingItem.Quantity + quantity;

            // Check stock for new total quantity
            if (product.StockQuantity < newQuantity)
                throw new InvalidOperationException("Product.InsufficientStock");

            existingItem.Quantity = newQuantity;
            existingItem.UnitPrice = product.Price.Amount; // Update price to latest
        }
        else
        {
            // Add new item
            cart.Items.Add(new ShoppingCartItem
            {
                ProductId = productId,
                ProductName = product.Name,
                ProductSku = product.Sku,
                UnitPrice = product.Price.Amount,
                Currency = product.Price.Currency,
                Quantity = quantity,
                ProductImageUrl = product.MainImageUrl,
                AddedAt = DateTime.UtcNow
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;

        // 6. Save to Redis
        var key = $"{CartKeyPrefix}{userId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        // 1. Validate quantity
        if (quantity < 0)
            throw new InvalidOperationException("Cart.InvalidQuantity");

        // 2. Get cart
        var cart = await GetCartAsync(userId, cancellationToken);

        // 3. Find item
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Cart.ItemNotFound");

        // 4. If quantity is 0, remove item
        if (quantity == 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            // 5. Check stock
            var product = await GetProductWithCacheAsync(productId, cancellationToken);
            if (product == null || !product.IsActive)
                throw new InvalidOperationException("Product.NotFound");

            if (product.StockQuantity < quantity)
                throw new InvalidOperationException("Product.InsufficientStock");

            // 6. Update quantity and price
            item.Quantity = quantity;
            item.UnitPrice = product.Price.Amount; // Update to latest price
        }

        cart.UpdatedAt = DateTime.UtcNow;

        // 7. Save to Redis
        var key = $"{CartKeyPrefix}{userId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, cancellationToken);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.Items.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;

            var key = $"{CartKeyPrefix}{userId}";
            await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
        }
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"{CartKeyPrefix}{userId}";
        await _cacheService.RemoveAsync(key, cancellationToken);
    }

    public async Task<int> GetCartItemCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, cancellationToken);
        return cart.TotalItemCount;
    }

    #endregion

    #region Guest Cart Operations

    public async Task<ShoppingCart> GetGuestCartAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        var key = $"{GuestCartKeyPrefix}{sessionId}";
        var cart = await _cacheService.GetAsync<ShoppingCart>(key, cancellationToken);

        if (cart == null)
        {
            cart = new ShoppingCart { UserId = Guid.Empty }; // Guid.Empty indicates guest
            await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
        }

        return cart;
    }

    public async Task AddGuestItemAsync(string sessionId, Guid productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        // 1. Validate quantity
        if (quantity <= 0)
            throw new InvalidOperationException("Cart.InvalidQuantity");

        // 2. Get product
        var product = await GetProductWithCacheAsync(productId, cancellationToken);
        if (product == null || !product.IsActive)
            throw new InvalidOperationException("Product.NotFound");

        // 3. Check stock
        if (product.StockQuantity < quantity)
            throw new InvalidOperationException("Product.InsufficientStock");

        // 4. Get guest cart
        var cart = await GetGuestCartAsync(sessionId, cancellationToken);

        // 5. Add or update item
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + quantity;
            if (product.StockQuantity < newQuantity)
                throw new InvalidOperationException("Product.InsufficientStock");

            existingItem.Quantity = newQuantity;
            existingItem.UnitPrice = product.Price.Amount;
        }
        else
        {
            cart.Items.Add(new ShoppingCartItem
            {
                ProductId = productId,
                ProductName = product.Name,
                ProductSku = product.Sku,
                UnitPrice = product.Price.Amount,
                Currency = product.Price.Currency,
                Quantity = quantity,
                ProductImageUrl = product.MainImageUrl,
                AddedAt = DateTime.UtcNow
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;

        // 6. Save to Redis
        var key = $"{GuestCartKeyPrefix}{sessionId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task UpdateGuestItemQuantityAsync(string sessionId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        if (quantity < 0)
            throw new InvalidOperationException("Cart.InvalidQuantity");

        var cart = await GetGuestCartAsync(sessionId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Cart.ItemNotFound");

        if (quantity == 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            var product = await GetProductWithCacheAsync(productId, cancellationToken);
            if (product == null || !product.IsActive)
                throw new InvalidOperationException("Product.NotFound");

            if (product.StockQuantity < quantity)
                throw new InvalidOperationException("Product.InsufficientStock");

            item.Quantity = quantity;
            item.UnitPrice = product.Price.Amount;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        var key = $"{GuestCartKeyPrefix}{sessionId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task RemoveGuestItemAsync(string sessionId, Guid productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        var cart = await GetGuestCartAsync(sessionId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.Items.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;

            var key = $"{GuestCartKeyPrefix}{sessionId}";
            await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
        }
    }

    public async Task ClearGuestCartAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        var key = $"{GuestCartKeyPrefix}{sessionId}";
        await _cacheService.RemoveAsync(key, cancellationToken);
    }

    #endregion

    #region Cart Merge

    public async Task MergeGuestCartAsync(string guestSessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(guestSessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(guestSessionId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        // 1. Get guest cart
        var guestCart = await GetGuestCartAsync(guestSessionId, cancellationToken);
        if (guestCart.Items.Count == 0)
            return; // Nothing to merge

        // 2. Get user cart
        var userCart = await GetCartAsync(userId, cancellationToken);

        // 3. Merge items
        foreach (var guestItem in guestCart.Items)
        {
            var existingUserItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
            if (existingUserItem != null)
            {
                // Merge quantities (use latest price)
                existingUserItem.Quantity += guestItem.Quantity;

                // Validate stock for merged quantity
                var product = await GetProductWithCacheAsync(guestItem.ProductId, cancellationToken);
                if (product != null && product.StockQuantity < existingUserItem.Quantity)
                {
                    // If stock insufficient, use max available
                    existingUserItem.Quantity = product.StockQuantity;
                }

                existingUserItem.UnitPrice = guestItem.UnitPrice; // Use latest price
            }
            else
            {
                // Add guest item to user cart
                userCart.Items.Add(guestItem);
            }
        }

        userCart.UpdatedAt = DateTime.UtcNow;

        // 4. Save merged user cart
        var userKey = $"{CartKeyPrefix}{userId}";
        await _cacheService.SetAsync(userKey, userCart, CartExpiration, cancellationToken);

        // 5. Clear guest cart
        await ClearGuestCartAsync(guestSessionId, cancellationToken);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Get product from cache or DB (cache-aside pattern)
    /// Cache key: product:detail:{productId}
    /// TTL: 1 hour
    /// </summary>
    private async Task<Product?> GetProductWithCacheAsync(Guid productId, CancellationToken cancellationToken)
    {
        var cacheKey = $"product:detail:{productId}";

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await _productRepository.GetByIdAsync(productId, cancellationToken),
            TimeSpan.FromHours(1),
            cancellationToken);
    }

    #endregion
}
