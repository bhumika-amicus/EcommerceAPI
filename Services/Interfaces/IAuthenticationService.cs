using EcommerceAPI.DTOs.Authentication;

namespace EcommerceAPI.Services;

public interface IAuthenticationService
{
    Task<RegisterResponseDto> RegisterAsync( RegisterDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync( LoginDto dto, CancellationToken cancellationToken = default);
}