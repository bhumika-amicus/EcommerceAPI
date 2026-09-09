
namespace EcommerceAPI.DTOs.Orders;

public class CreateOrderDto
{
    public int ShippingMethodId { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;
}

