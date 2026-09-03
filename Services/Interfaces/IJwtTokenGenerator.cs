namespace EcommerceAPI.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(
        int userId,
        string email,
        string fullName,
        IEnumerable<string> roles);
}