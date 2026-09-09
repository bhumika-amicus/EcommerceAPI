
using System.Text.Json.Serialization;

namespace EcommerceAPI.DTOs.Orders;

public class OrderDto
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public decimal Subtotal { get; set; }

    public int ShippingMethodId { get; set; }

    public string ShippingMethod { get; set; } = string.Empty;

    public decimal ShippingFee { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    public string ShippingAddress { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OrderItemDto>? Items { get; set; }
}

