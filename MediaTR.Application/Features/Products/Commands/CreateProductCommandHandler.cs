using MediatR;
using MediaTR.Application.BusinessLogic;
using MediaTR.Application.Features.Products.DTOs;

namespace MediaTR.Application.Features.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    private readonly ProductBusinessLogic _productBusinessLogic;

    public CreateProductCommandHandler(ProductBusinessLogic productBusinessLogic)
    {
        _productBusinessLogic = productBusinessLogic;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Delegate to business logic
        var product = _productBusinessLogic.CreateProduct(
            request.Name,
            request.Description,
            request.CategoryId,
            request.Price,
            request.Sku,
            request.StockQuantity,
            request.Weight
        );

        // TODO: Save to repository
        // await _productRepository.AddAsync(product, cancellationToken);

        // Return result
        return new CreateProductResult(
            product.Id,
            product.Name,
            product.Slug,
            product.Price,
            product.Sku,
            product.IsActive
        );
    }
}