using MediaTR.Domain.Entities.InMemory;
using MediatR;

namespace MediaTR.Application.Features.Cart.Commands.AddToCart;

/// <summary>
/// Add item to shopping cart command
/// </summary>
public record AddToCartCommand(
    Guid ProductId,
    int Quantity = 1) : IRequest<ShoppingCart>;
