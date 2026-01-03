using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Products.Queries.GetFeaturedProducts;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Products.Queries.SearchProducts;

/// <summary>
/// Search products query handler with pagination (uses Redis cache - 15 min TTL per page)
/// </summary>
internal sealed class SearchProductsQueryHandler : IQueryHandler<SearchProductsQuery, SearchProductsResult>
{
    private readonly IProductRepository _productRepository;

    public SearchProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<SearchProductsResult>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Validate search term
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return Error.Validation("Search.EmptyTerm", "Search term cannot be empty");

        // Search with pagination (Redis cache → MongoDB fallback)
        var searchResult = await _productRepository.SearchProductsPaginatedAsync(
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Map to DTOs
        var productDtos = searchResult.Products.Select(p => new ProductSummaryDto(
            p.Id,
            p.Name,
            p.Slug,
            p.Price,
            p.IsInStock,
            p.MainImageUrl
        )).ToList();

        return new SearchProductsResult(
            productDtos,
            searchResult.TotalCount,
            searchResult.Page,
            searchResult.PageSize,
            searchResult.TotalPages,
            request.SearchTerm);
    }
}
