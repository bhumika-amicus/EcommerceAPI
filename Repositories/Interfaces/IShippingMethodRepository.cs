using EcommerceAPI.DTOs.Checkout;

namespace EcommerceAPI.Repositories;

public interface IShippingMethodRepository
{
    Task<ShippingMethodDto?> GetShippingMethodByIdAsync( int shippingMethodId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShippingMethodDto>> GetAllShippingMethodsAsync( CancellationToken cancellationToken = default);
}