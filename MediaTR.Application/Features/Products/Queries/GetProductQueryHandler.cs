using MediatR;

namespace MediaTR.Application.Features.Products.Queries;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, GetProductResult?>
{
    // TODO: Add repository injection
    // private readonly IProductRepository _productRepository;

    public GetProductQueryHandler()
    {
        // _productRepository = productRepository;
    }

    public async Task<GetProductResult?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        // TODO: Get from repository
        // var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        // if (product == null)
        //     return null;

        // For now, return mock data
        // return new GetProductResult(
        //     product.Id,
        //     product.Name,
        //     product.Description,
        //     product.Slug,
        //     product.CategoryId,
        //     product.Price,
        //     product.StockQuantity,
        //     product.Sku,
        //     product.IsActive,
        //     product.IsFeatured,
        //     product.IsInStock,
        //     product.MainImageUrl
        // );

        return null; // TODO: Implement when repository is ready
    }
}