
namespace EcommerceAPI.DTOs.Orders;

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}

