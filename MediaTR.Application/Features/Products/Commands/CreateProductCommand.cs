using MediatR;
using MediaTR.Application.Features.Products.DTOs;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Products.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    Guid CategoryId,
    Money Price,
    string Sku,
    int StockQuantity,
    double Weight = 0
) : IRequest<CreateProductResult>;