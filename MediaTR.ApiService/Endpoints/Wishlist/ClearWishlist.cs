using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Wishlist.Commands.ClearWishlist;

namespace MediaTR.ApiService.Endpoints.Wishlist;

/// <summary>
/// Clear wishlist endpoint
/// </summary>
internal sealed class ClearWishlist : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/wishlist", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ClearWishlistCommand();
            var result = await sender.Send(command, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("ClearWishlist")
        .WithSummary("Clear wishlist")
        .WithDescription("Remove all products from the user's wishlist")
        .WithOpenApi()
        .Produces<bool>(StatusCodes.Status200OK)
        .RequireAuthorization(); // Wishlist requires authentication
    }
}
