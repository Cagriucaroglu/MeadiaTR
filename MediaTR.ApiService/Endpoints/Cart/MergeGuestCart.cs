using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.Application.Features.Cart.Commands.MergeGuestCart;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Cart;

/// <summary>
/// Merge guest cart into user cart after login
/// </summary>
internal sealed class MergeGuestCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/cart/merge", async (
            [FromBody] MergeGuestCartRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new MergeGuestCartCommand(request.GuestSessionId);
            var result = await sender.Send(command, cancellationToken);

            return Results.Ok(new { Success = result, Message = "Guest cart merged successfully" });
        })
        .WithName("MergeGuestCart")
        .WithSummary("Merge guest cart")
        .WithDescription("Merge guest cart items into authenticated user cart after login")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization(); // Must be authenticated
    }
}

/// <summary>
/// Merge guest cart request DTO
/// </summary>
public record MergeGuestCartRequest(string GuestSessionId);
