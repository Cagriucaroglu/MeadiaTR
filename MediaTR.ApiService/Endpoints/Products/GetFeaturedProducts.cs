using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Products.Queries.GetFeaturedProducts;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Products;

/// <summary>
/// Get featured products endpoint
/// </summary>
internal sealed class GetFeaturedProducts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/featured", async (
            [FromQuery] int count,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Default count = 10 if not provided
            GetFeaturedProductsQuery query = new(count == 0 ? 10 : count);

            Result<GetFeaturedProductsResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetFeaturedProducts")
        .WithSummary("Get featured products")
        .WithDescription("Retrieves featured products (cached for 15 minutes). Default count is 10.")
        .WithOpenApi()
        .Produces<GetFeaturedProductsResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
