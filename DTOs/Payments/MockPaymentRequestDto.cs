namespace EcommerceAPI.DTOs.Payments;
public class MockPaymentRequestDto {
    public decimal Amount { get; set; } 
    public string PaymentMethod { get; set; } = string.Empty;

    public bool SimulateFailure { get; set; }
}