namespace MediaTR.Domain.Entities.InMemory;

/// <summary>
/// Shopping cart item (Redis-only, not persisted in MongoDB)
/// </summary>
public class ShoppingCartItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int Quantity { get; set; }
    public string? ProductImageUrl { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalPrice => UnitPrice * Quantity;
}

/// <summary>
/// Shopping cart (Redis-only, 30 days TTL)
/// Stored in Redis with key: cart:user:{userId} or cart:guest:{sessionId}
/// </summary>
public class ShoppingCart
{
    public Guid UserId { get; set; } // Guid.Empty for guest users
    public List<ShoppingCartItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int TotalItemCount => Items.Sum(i => i.Quantity);
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}
