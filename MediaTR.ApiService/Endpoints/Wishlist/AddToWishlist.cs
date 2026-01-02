using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Wishlist.Commands.AddToWishlist;

namespace MediaTR.ApiService.Endpoints.Wishlist;

/// <summary>
/// Add product to wishlist endpoint
/// </summary>
internal sealed class AddToWishlist : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/wishlist/{productId:guid}", async (
            Guid productId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AddToWishlistCommand(productId);
            var result = await sender.Send(command, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("AddToWishlist")
        .WithSummary("Add product to wishlist")
        .WithDescription("Add a product to the user's wishlist")
        .WithOpenApi()
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(); // Wishlist requires authentication
    }
}
