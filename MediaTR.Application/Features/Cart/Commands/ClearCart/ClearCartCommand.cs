using MediatR;

namespace MediaTR.Application.Features.Cart.Commands.ClearCart;

/// <summary>
/// Clear all items from shopping cart command
/// </summary>
public record ClearCartCommand() : IRequest<bool>;
