using EcommerceAPI.DTOs.Authentication;

namespace EcommerceAPI.Repositories;

public interface IUserRepository
{
    Task<int> CreateUserAsync( RegisterDto dto, string passwordHash, CancellationToken cancellationToken = default);

    Task<UserLoginDataDto?> GetUserByEmailAsync( string email, CancellationToken cancellationToken = default);
}