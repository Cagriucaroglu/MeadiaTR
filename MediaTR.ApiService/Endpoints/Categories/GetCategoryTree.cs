using MediatR;
using MediaTR.ApiService.Extensions;
using MediaTR.Application.Features.Categories.Queries.GetCategoryTree;
using MediaTR.SharedKernel.Localization;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Endpoints.Categories;

/// <summary>
/// Get category tree endpoint with hierarchical structure
/// </summary>
internal sealed class GetCategoryTree : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories/tree", async (
            ISender sender,
            ILocalizationService localizationService,
            CancellationToken cancellationToken) =>
        {
            GetCategoryTreeQuery query = new();

            Result<CategoryTreeResult> result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.ToResponse(localizationService);
        })
        .WithName("GetCategoryTree")
        .WithSummary("Get category tree with hierarchical structure")
        .WithDescription(@"
Get full category tree with all children in hierarchical structure.
- Cached for 30 minutes
- Automatically sorted by SortOrder
- Only returns active categories
- Example: /api/categories/tree
        ")
        .WithOpenApi()
        .Produces<CategoryTreeResult>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
