using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Cart.Commands.ClearCart;

namespace MediaTR.ApiService.Endpoints.Cart;

/// <summary>
/// Clear cart endpoint
/// </summary>
internal sealed class ClearCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/cart", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ClearCartCommand();
            var result = await sender.Send(command, cancellationToken);

            return Results.Ok(new { Success = result, Message = "Cart cleared successfully" });
        })
        .WithName("ClearCart")
        .WithSummary("Clear cart")
        .WithDescription("Remove all items from the shopping cart")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}
