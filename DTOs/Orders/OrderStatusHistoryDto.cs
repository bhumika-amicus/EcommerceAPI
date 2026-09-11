namespace EcommerceAPI.DTOs.Orders;

public class OrderStatusHistoryDto
{
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
