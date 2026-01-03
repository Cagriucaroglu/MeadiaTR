using MediatR;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Categories.Queries.GetRootCategories;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Categories;

/// <summary>
/// Get root categories endpoint
/// </summary>
internal sealed class GetRootCategories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories", async (
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetRootCategoriesQuery query = new();

            Result<RootCategoriesResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetRootCategories")
        .WithSummary("Get root categories")
        .WithDescription(@"
Get top-level categories only (no children included).
- Cached for 30 minutes
- Returns only active categories
- Sorted by SortOrder
- Use /api/categories/tree for full hierarchical structure
- Example: /api/categories
        ")
        .WithOpenApi()
        .Produces<RootCategoriesResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
