using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Cart.Commands.UpdateCartItem;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Cart;

/// <summary>
/// Update cart item quantity endpoint
/// </summary>
internal sealed class UpdateCartItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/cart/items/{productId:guid}", async (
            [FromRoute] Guid productId,
            [FromBody] UpdateCartItemRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCartItemCommand(productId, request.Quantity);
            var cart = await sender.Send(command, cancellationToken);

            return Results.Ok(cart);
        })
        .WithName("UpdateCartItem")
        .WithSummary("Update cart item quantity")
        .WithDescription("Update the quantity of an item in the cart. Set quantity to 0 to remove the item.")
        .WithOpenApi()
        .Produces<Domain.Entities.InMemory.ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}

/// <summary>
/// Update cart item request DTO
/// </summary>
public record UpdateCartItemRequest(int Quantity);
