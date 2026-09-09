namespace EcommerceAPI.DTOs.Checkout;

public class CheckoutRequestDto
{
    public int ShippingMethodId { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;
}