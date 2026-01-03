using MediaTR.Domain.Entities;
using MediaTR.Domain.Entities.InMemory;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;

namespace MediaTR.Infrastructure.Services.Cart;

/// <summary>
/// Shopping cart service using Redis for ultra-fast cart operations
/// Target: < 10ms operations for 1M+ users
/// Responsibilities: Cache orchestration ONLY (business logic delegated to ShoppingCartBusinessLogic)
/// </summary>
public class ShoppingCartService : IShoppingCartService
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;
    private readonly ShoppingCartBusinessLogic _businessLogic;
    private const string CartKeyPrefix = "cart:user:";
    private const string GuestCartKeyPrefix = "cart:guest:";
    private static readonly TimeSpan CartExpiration = TimeSpan.FromDays(30);

    public ShoppingCartService(
        ICacheService cacheService,
        IProductRepository productRepository,
        ShoppingCartBusinessLogic businessLogic)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
        _businessLogic = businessLogic;
    }

    #region User Cart Operations

    public async Task<ShoppingCart> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"{CartKeyPrefix}{userId}";
        var cart = await _cacheService.GetAsync<ShoppingCart>(key, cancellationToken);

        if (cart == null)
        {
            // Use BusinessLogic to create cart
            cart = _businessLogic.CreateCart(userId);
            await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
        }

        return cart;
    }

    public async Task AddItemAsync(Guid userId, Guid productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        // 1. Get product from cache or DB
        var product = await GetProductWithCacheAsync(productId, cancellationToken);
        if (product == null)
            throw new InvalidOperationException("Product.NotFound");

        // 2. Get cart
        var cart = await GetCartAsync(userId, cancellationToken);

        // 3. Use BusinessLogic to add/update item (includes all validations)
        _businessLogic.AddOrUpdateItem(cart, product, quantity);

        // 4. Save to Redis
        var key = $"{CartKeyPrefix}{userId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        // 1. Get cart
        var cart = await GetCartAsync(userId, cancellationToken);

        // 2. Find item
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Cart.ItemNotFound");

        // 3. If quantity is 0, remove item
        if (quantity == 0)
        {
            _businessLogic.RemoveItem(cart, productId);
        }
        else
        {
            // 4. Get product for validation
            var product = await GetProductWithCacheAsync(productId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException("Product.NotFound");

            // 5. Use BusinessLogic to update quantity (includes validation)
            _businessLogic.UpdateItemQuantity(item, product, quantity);
            cart.UpdatedAt = DateTime.UtcNow;
        }

        // 6. Save to Redis
        var key = $"{CartKeyPrefix}{userId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, cancellationToken);

        // Use BusinessLogic to remove item
        _businessLogic.RemoveItem(cart, productId);

        var key = $"{CartKeyPrefix}{userId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
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
            cart = _businessLogic.CreateCart(Guid.Empty); // Guid.Empty for guests
            await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
        }

        return cart;
    }

    public async Task AddGuestItemAsync(string sessionId, Guid productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        // 1. Get product
        var product = await GetProductWithCacheAsync(productId, cancellationToken);
        if (product == null)
            throw new InvalidOperationException("Product.NotFound");

        // 2. Get guest cart
        var cart = await GetGuestCartAsync(sessionId, cancellationToken);

        // 3. Use BusinessLogic to add/update item
        _businessLogic.AddOrUpdateItem(cart, product, quantity);

        // 4. Save to Redis
        var key = $"{GuestCartKeyPrefix}{sessionId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task UpdateGuestItemQuantityAsync(string sessionId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        var cart = await GetGuestCartAsync(sessionId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Cart.ItemNotFound");

        if (quantity == 0)
        {
            _businessLogic.RemoveItem(cart, productId);
        }
        else
        {
            var product = await GetProductWithCacheAsync(productId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException("Product.NotFound");

            _businessLogic.UpdateItemQuantity(item, product, quantity);
            cart.UpdatedAt = DateTime.UtcNow;
        }

        var key = $"{GuestCartKeyPrefix}{sessionId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
    }

    public async Task RemoveGuestItemAsync(string sessionId, Guid productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        var cart = await GetGuestCartAsync(sessionId, cancellationToken);
        _businessLogic.RemoveItem(cart, productId);

        var key = $"{GuestCartKeyPrefix}{sessionId}";
        await _cacheService.SetAsync(key, cart, CartExpiration, cancellationToken);
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

        // 3. Get all products for validation
        var productIds = guestCart.Items.Select(i => i.ProductId).Distinct();
        var products = new List<Product>();
        foreach (var productId in productIds)
        {
            var product = await GetProductWithCacheAsync(productId, cancellationToken);
            if (product != null)
            {
                products.Add(product);
            }
        }

        // 4. Use BusinessLogic to merge carts
        _businessLogic.MergeGuestCart(userCart, guestCart, products);

        // 5. Save merged user cart
        var userKey = $"{CartKeyPrefix}{userId}";
        await _cacheService.SetAsync(userKey, userCart, CartExpiration, cancellationToken);

        // 6. Clear guest cart
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
