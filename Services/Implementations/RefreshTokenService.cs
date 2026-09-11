using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceAPI.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IMemoryCache _cache;

    public RefreshTokenService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> GenerateAsync( int userId,  CancellationToken cancellationToken = default)
    {
        // Generate a cryptographically secure random token.
        var refreshToken = Convert.ToBase64String( RandomNumberGenerator.GetBytes(64));

        // Use the refresh token itself as the cache key.
        var cacheKey = $"refresh:{refreshToken}";

        // Store the user ID against the refresh token.
        _cache.Set(cacheKey, userId, TimeSpan.FromDays(7));

        return Task.FromResult(refreshToken);
    }

    public Task<int?> ValidateAsync( string refreshToken, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"refresh:{refreshToken}";

        if (_cache.TryGetValue<int>(cacheKey, out var userId))
        {
            return Task.FromResult<int?>(userId);
        }

        return Task.FromResult<int?>(null);
    }

    public Task RevokeAsync( string refreshToken, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"refresh:{refreshToken}";

        _cache.Remove(cacheKey);

        return Task.CompletedTask;
    }
}

