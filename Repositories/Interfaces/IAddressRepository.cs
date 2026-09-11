
using EcommerceAPI.DTOs.Addresses;
using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetAddressByUserIdAsync( int userId, CancellationToken cancellationToken = default);

    Task<int> CreateAddressAsync( int userId, AddressReqDto dto, CancellationToken cancellationToken = default);

    Task<bool> UpdateAddressAsync( int userId, AddressReqDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAddressAsync( int userId, CancellationToken cancellationToken = default);
}

