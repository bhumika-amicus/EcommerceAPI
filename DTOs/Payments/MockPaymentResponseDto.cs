public class MockPaymentResponseDto
{
    public bool Success { get; set; }

    public string TransactionReference { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}