namespace EcommerceAPI.DTOs.Payments
{
    public class PaymentRequestDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public bool SimulateFailure { get; set; }
    }
}

