namespace EcommerceAPI.DTOs.Payments;
public class PaymentDto {
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string? TransactionReference { get; set; }
    public decimal Amount { get; set; } 
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } public DateTime? UpdatedAt { get; set; } }