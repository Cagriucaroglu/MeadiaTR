using MediatR;

namespace MediaTR.Application.Features.Wishlist.Queries.GetWishlist;

/// <summary>
/// Get user's wishlist with product details
/// </summary>
public record GetWishlistQuery : IRequest<WishlistDto?>;
