using MediatR;

namespace MediaTR.Application.Features.Wishlist.Commands.AddToWishlist;

/// <summary>
/// Add product to wishlist command
/// </summary>
public record AddToWishlistCommand(Guid ProductId) : IRequest<bool>;
