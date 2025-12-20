using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Cart.Commands.AddToCart;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Cart;

/// <summary>
/// Add item to cart endpoint
/// </summary>
internal sealed class AddToCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/cart/items", async (
            [FromBody] AddToCartRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AddToCartCommand(request.ProductId, request.Quantity);
            var cart = await sender.Send(command, cancellationToken);

            return Results.Ok(cart);
        })
        .WithName("AddToCart")
        .WithSummary("Add item to cart")
        .WithDescription("Add a product to the shopping cart. Supports both authenticated users and guests (via X-Session-Id header)")
        .WithOpenApi()
        .Produces<Domain.Entities.InMemory.ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous(); // Guests can add to cart
    }
}

/// <summary>
/// Add to cart request DTO
/// </summary>
public record AddToCartRequest(Guid ProductId, int Quantity = 1);
