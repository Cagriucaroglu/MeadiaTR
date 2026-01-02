using MediatR;

namespace MediaTR.Application.Features.Wishlist.Commands.RemoveFromWishlist;

/// <summary>
/// Remove product from wishlist command
/// </summary>
public record RemoveFromWishlistCommand(Guid ProductId) : IRequest<bool>;
