
namespace EcommerceAPI.Models.Orders;

public class CreateOrderModel
{
    public int CustomerId { get; set; }

    public int ShippingMethodId { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public List<CreateOrderItemModel> Items { get; set; } = new();
}

public class CreateOrderItemModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}

