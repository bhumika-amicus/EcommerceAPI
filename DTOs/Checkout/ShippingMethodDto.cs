namespace EcommerceAPI.DTOs.Checkout;

public class ShippingMethodDto
{
    public int ShippingMethodId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Fee { get; set; }

    public int EstimatedDeliveryDays { get; set; }
}