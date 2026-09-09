namespace EcommerceAPI.DTOs.Checkout;

public class CheckoutDto
{
    public List<CheckoutItemDto> Items { get; set; } = new();

    public string ShippingAddress { get; set; } = string.Empty;

    public int ShippingMethodId { get; set; }

    public string ShippingMethod { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public List<string> PriceChangeMessages { get; set; } = new();
}