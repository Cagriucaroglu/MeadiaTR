using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Products.Queries.GetFeaturedProducts;

/// <summary>
/// Get featured products query handler (uses Redis cache - 15 min TTL)
/// </summary>
internal sealed class GetFeaturedProductsQueryHandler : IQueryHandler<GetFeaturedProductsQuery, GetFeaturedProductsResult>
{
    private readonly IProductRepository _productRepository;

    public GetFeaturedProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetFeaturedProductsResult>> Handle(
        GetFeaturedProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Get from repository (Redis cache → MongoDB fallback)
        var products = await _productRepository.GetFeaturedProductsAsync(request.Count, cancellationToken);

        var productDtos = products.Select(p => new ProductSummaryDto(
            p.Id,
            p.Name,
            p.Slug,
            p.Price,
            p.IsInStock,
            p.MainImageUrl
        )).ToList();

        return new GetFeaturedProductsResult(productDtos, productDtos.Count);
    }
}
