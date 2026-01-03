using MediatR;
using MediaTR.ApiService.Endpoints;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Products.Queries.SearchProducts;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Products;

/// <summary>
/// Search products endpoint with pagination
/// </summary>
internal sealed class SearchProducts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/search", async (
            [FromQuery] string q,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            // Default values
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 20;

            SearchProductsQuery query = new(q, page, pageSize);

            Result<SearchProductsResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("SearchProducts")
        .WithSummary("Search products with pagination")
        .WithDescription(@"
Search products by name or description with pagination.
- Cached for 15 minutes per page
- Default page size is 20 products
- Max page size is 100 products
- Example: /api/products/search?q=iphone&page=1&pageSize=20
        ")
        .WithOpenApi()
        .Produces<SearchProductsResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
