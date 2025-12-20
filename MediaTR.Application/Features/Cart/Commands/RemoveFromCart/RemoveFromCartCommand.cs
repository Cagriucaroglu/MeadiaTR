using MediaTR.Domain.Entities.InMemory;
using MediatR;

namespace MediaTR.Application.Features.Cart.Commands.RemoveFromCart;

/// <summary>
/// Remove item from shopping cart command
/// </summary>
public record RemoveFromCartCommand(Guid ProductId) : IRequest<ShoppingCart>;
