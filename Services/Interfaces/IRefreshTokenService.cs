namespace EcommerceAPI.Services
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateAsync(int userId, CancellationToken cancellationToken = default);
        Task<int?> ValidateAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
