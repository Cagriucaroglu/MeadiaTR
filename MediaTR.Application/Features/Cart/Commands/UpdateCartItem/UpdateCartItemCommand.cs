using MediaTR.Domain.Entities.InMemory;
using MediatR;

namespace MediaTR.Application.Features.Cart.Commands.UpdateCartItem;

/// <summary>
/// Update cart item quantity command
/// </summary>
public record UpdateCartItemCommand(
    Guid ProductId,
    int Quantity) : IRequest<ShoppingCart>;
