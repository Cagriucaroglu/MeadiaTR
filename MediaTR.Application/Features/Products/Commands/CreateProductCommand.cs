using MediaTR.Application.Abstractions.Messaging;
using MediaTR.Domain.ValueObjects;

namespace MediaTR.Application.Features.Products.Commands;

/// <summary>
/// Create product DTO
/// </summary>
public record CreateProductDto(
    string Name,
    string Description,
    Guid CategoryId,
    Money Price,
    string Sku,
    int StockQuantity,
    double Weight = 0);

/// <summary>
/// Create product command using CommandWrapper pattern
/// </summary>
public sealed record CreateProductCommand(
    CreateProductDto Request,
    Guid CorrelationId)
    : CommandWrapper<CreateProductDto, Guid>(Request, CorrelationId);