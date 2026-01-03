using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Products.Queries;
using MediaTR.Application.Features.Products.Queries.GetProductBySlug;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Products;

/// <summary>
/// Get product by slug endpoint (SEO-friendly URL)
/// </summary>
internal sealed class GetProductBySlug : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/slug/{slug}", async (
            string slug,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetProductBySlugQuery query = new(slug);

            Result<GetProductResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetProductBySlug")
        .WithSummary("Get product by slug")
        .WithDescription("Retrieves a product by its SEO-friendly slug (cached for 1 hour). Example: /api/products/slug/iphone-15-pro-max-256gb")
        .WithOpenApi()
        .Produces<GetProductResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
