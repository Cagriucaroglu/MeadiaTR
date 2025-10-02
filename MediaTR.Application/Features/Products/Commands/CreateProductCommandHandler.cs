using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Application.BusinessLogic;
using MediaTR.Application.Features.Products.DTOs;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Features.Products.Commands;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    private readonly ProductBusinessLogic _productBusinessLogic;

    public CreateProductCommandHandler(ProductBusinessLogic productBusinessLogic)
    {
        _productBusinessLogic = productBusinessLogic;
    }

    public async Task<Result<CreateProductResult>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Delegate to business logic
        var product = _productBusinessLogic.CreateProduct(
            request.Name,
            request.Description,
            request.CategoryId,
            request.Price,
            request.Sku,
            request.StockQuantity,
            request.CorrelationId,
            request.Weight
        );

        // TODO: Save to repository
        // await _productRepository.AddAsync(product, cancellationToken);

        // Return result using implicit operator
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