using MediaTR.Domain.Entities.InMemory;
using MediatR;

namespace MediaTR.Application.Features.Cart.Queries.GetCart;

/// <summary>
/// Get shopping cart query
/// </summary>
public record GetCartQuery() : IRequest<ShoppingCart>;
