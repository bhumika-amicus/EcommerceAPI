using EcommerceAPI.DTOs.Checkout;

namespace EcommerceAPI.Services;

public interface ICheckoutService
{
    Task<CheckoutDto> PreviewCheckoutAsync( int customerId, CheckoutRequestDto dto, CancellationToken cancellationToken = default);
}