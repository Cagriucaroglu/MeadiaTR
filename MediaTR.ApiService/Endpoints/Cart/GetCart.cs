using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Cart.Queries.GetCart;

namespace MediaTR.ApiService.Endpoints.Cart;

/// <summary>
/// Get shopping cart endpoint
/// </summary>
internal sealed class GetCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/cart", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCartQuery();
            var cart = await sender.Send(query, cancellationToken);

            return Results.Ok(cart);
        })
        .WithName("GetCart")
        .WithSummary("Get shopping cart")
        .WithDescription("Get the current user's shopping cart or guest cart based on session ID")
        .WithOpenApi()
        .Produces<Domain.Entities.InMemory.ShoppingCart>(StatusCodes.Status200OK)
        .AllowAnonymous(); // Guests can view cart
    }
}
