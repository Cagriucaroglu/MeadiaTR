using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.Features.Products.Queries.GetFeaturedProducts;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Get products by category query handler (uses Redis cache - 30 min TTL)
/// </summary>
internal sealed class GetProductsByCategoryQueryHandler : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByCategoryQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductsByCategoryResult>> Handle(
        GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        // Get from repository (Redis cache → MongoDB fallback)
        var products = await _productRepository.GetByCategoryIdAsync(request.CategoryId, cancellationToken);

        var productDtos = products.Select(p => new ProductSummaryDto(
            p.Id,
            p.Name,
            p.Slug,
            p.Price,
            p.IsInStock,
            p.MainImageUrl
        )).ToList();

        return new GetProductsByCategoryResult(request.CategoryId, productDtos, productDtos.Count);
    }
}
