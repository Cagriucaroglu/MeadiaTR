using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Cart.Commands.RemoveFromCart;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Cart;

/// <summary>
/// Remove item from cart endpoint
/// </summary>
internal sealed class RemoveFromCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/cart/items/{productId:guid}", async (
            [FromRoute] Guid productId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveFromCartCommand(productId);
            var cart = await sender.Send(command, cancellationToken);

            return Results.Ok(cart);
        })
        .WithName("RemoveFromCart")
        .WithSummary("Remove item from cart")
        .WithDescription("Remove a specific product from the shopping cart")
        .WithOpenApi()
        .Produces<Domain.Entities.InMemory.ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}
