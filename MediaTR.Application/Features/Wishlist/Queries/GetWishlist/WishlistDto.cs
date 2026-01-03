namespace MediaTR.Application.Features.Wishlist.Queries.GetWishlist;

/// <summary>
/// Wishlist product DTO
/// </summary>
public record WishlistProductDto(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsActive,
    int StockQuantity);

/// <summary>
/// Wishlist DTO with product details
/// </summary>
public record WishlistDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<WishlistProductDto> Products,
    int TotalCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
