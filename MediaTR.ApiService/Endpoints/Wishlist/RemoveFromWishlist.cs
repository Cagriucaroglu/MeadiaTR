using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Wishlist.Commands.RemoveFromWishlist;

namespace MediaTR.ApiService.Endpoints.Wishlist;

/// <summary>
/// Remove product from wishlist endpoint
/// </summary>
internal sealed class RemoveFromWishlist : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/wishlist/{productId:guid}", async (
            Guid productId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveFromWishlistCommand(productId);
            var result = await sender.Send(command, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("RemoveFromWishlist")
        .WithSummary("Remove product from wishlist")
        .WithDescription("Remove a product from the user's wishlist")
        .WithOpenApi()
        .Produces<bool>(StatusCodes.Status200OK)
        .RequireAuthorization(); // Wishlist requires authentication
    }
}
