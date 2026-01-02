using MediatR;

namespace MediaTR.Application.Features.Wishlist.Commands.ClearWishlist;

/// <summary>
/// Clear all products from wishlist command
/// </summary>
public record ClearWishlistCommand : IRequest<bool>;
