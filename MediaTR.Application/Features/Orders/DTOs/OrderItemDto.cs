namespace MediaTR.Application.Features.Orders.DTOs;

public class OrderItemDto
{
    public Guid ProductId { get; set; } 
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
}