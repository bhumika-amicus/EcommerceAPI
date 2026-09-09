
using System.Net.Http.Json;
using EcommerceAPI.DTOs.Payments;

namespace EcommerceAPI.Services;

public class MockPaymentClient : IMockPaymentClient
{
    private readonly HttpClient _httpClient;

    public MockPaymentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MockPaymentResponseDto> ProcessPaymentAsync(decimal amount, string paymentMethod, bool simulateFailure = false, CancellationToken cancellationToken = default)
    {
        var request = new MockPaymentRequestDto
        {
            Amount = amount,
            PaymentMethod = paymentMethod,
            SimulateFailure = simulateFailure
        };

        var response = await _httpClient.PostAsJsonAsync( "api/mock-payments", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MockPaymentResponseDto>( cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException( "Mock payment API returned an empty response.");
        }

        return result;
    }
}

