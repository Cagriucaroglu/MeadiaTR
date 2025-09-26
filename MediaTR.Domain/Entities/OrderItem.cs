using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; } 
    public Order? Order { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public Money UnitPrice { get; set; } 
    public int Quantity { get; set; }
    public Money DiscountAmount { get; set; } 

    public Money TotalPrice => Money.FromDecimal(
        (UnitPrice.Amount * Quantity) - DiscountAmount.Amount,
        UnitPrice.Currency
    );
}