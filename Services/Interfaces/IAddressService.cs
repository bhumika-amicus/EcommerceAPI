using EcommerceAPI.DTOs.Addresses;

namespace EcommerceAPI.Services;

public interface IAddressService
{
    Task<AddressDto?> GetAddressByUserIdAsync( int userId, CancellationToken cancellationToken = default);

    Task<AddressDto> SaveAddressAsync( int userId, AddressReqDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAddressAsync( int userId, CancellationToken cancellationToken = default);
}
