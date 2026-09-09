using EcommerceAPI.DTOs.Checkout;

namespace EcommerceAPI.Services;

public interface IShippingMethodService
{
    Task<ShippingMethodDto?> GetShippingMethodByIdAsync( int shippingMethodId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShippingMethodDto>> GetAllShippingMethodsAsync( CancellationToken cancellationToken = default);
}
