using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.Repositories;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Products.Queries.GetProductBySlug;

/// <summary>
/// Get product by slug query handler (uses Redis cache - 1 hour TTL)
/// SEO-friendly product detail endpoint
/// </summary>
internal sealed class GetProductBySlugQueryHandler : IQueryHandler<GetProductBySlugQuery, GetProductResult>
{
    private readonly IProductRepository _productRepository;

    public GetProductBySlugQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductResult>> Handle(
        GetProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        // Get from repository (Redis cache → MongoDB fallback)
        var product = await _productRepository.GetBySlugAsync(request.Slug, cancellationToken);

        if (product == null)
            return Error.NotFound("Product.NotFound", $"Product with slug '{request.Slug}' not found");

        return new GetProductResult(
            product.Id,
            product.Name,
            product.Description,
            product.Slug,
            product.CategoryId,
            product.Price,
            product.StockQuantity,
            product.Sku,
            product.IsActive,
            product.IsFeatured,
            product.IsInStock,
            product.MainImageUrl
        );
    }
}
