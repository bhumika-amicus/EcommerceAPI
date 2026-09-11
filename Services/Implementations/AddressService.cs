
using EcommerceAPI.DTOs.Addresses;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ILogger<AddressService> _logger;

    public AddressService(IAddressRepository addressRepository, ILogger<AddressService> logger)
    {
        _addressRepository = addressRepository;
        _logger = logger;
    }

    public async Task<AddressDto?> GetAddressByUserIdAsync( int userId,  CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching address for user {UserId}.", userId);
        var address = await _addressRepository.GetAddressByUserIdAsync( userId, cancellationToken);

        if (address == null)
        {
            return null;
        }

        return new AddressDto
        {
            AddressId = address.AddressId,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }

    public async Task<AddressDto> SaveAddressAsync( int userId, AddressReqDto dto,  CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving address for user {UserId}.", userId);
        var existingAddress =  await _addressRepository.GetAddressByUserIdAsync( userId, cancellationToken);

        if (existingAddress == null)
        {
            // Create a new address if it doesn't exist
            _logger.LogInformation("Creating new address for user {UserId}.", userId);
            var addressId = await _addressRepository.CreateAddressAsync(  userId, dto, cancellationToken);

            return new AddressDto
            {
                AddressId = addressId,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country
            };
        }
        // else if address already present then update the existing address
        _logger.LogInformation("Updating existing address for user {UserId}.", userId);
        var isUpdated = await _addressRepository.UpdateAddressAsync( userId,dto, cancellationToken);

        if (!isUpdated)
        {
            _logger.LogError("Failed to update address for user {UserId}.", userId);
            throw new InvalidOperationException( "Failed to update the user's address.");
        }

        return new AddressDto
        {
            AddressId = existingAddress.AddressId,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };
    }

    public async Task<bool> DeleteAddressAsync(  int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting address for user {UserId}.", userId);
        return await _addressRepository.DeleteAddressAsync(
            userId,
            cancellationToken);
    }
}

