using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Wishlist.Queries.GetWishlist;

namespace MediaTR.ApiService.Endpoints.Wishlist;

/// <summary>
/// Get wishlist endpoint
/// </summary>
internal sealed class GetWishlist : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/wishlist", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetWishlistQuery();
            var wishlist = await sender.Send(query, cancellationToken);

            return wishlist == null ? Results.NotFound() : Results.Ok(wishlist);
        })
        .WithName("GetWishlist")
        .WithSummary("Get wishlist")
        .WithDescription("Get the current user's wishlist with product details")
        .WithOpenApi()
        .Produces<WishlistDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(); // Wishlist requires authentication
    }
}
