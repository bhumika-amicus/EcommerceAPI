using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Authentication;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Identity;

namespace EcommerceAPI.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator , IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var passwordHash = _passwordHasher.Hash(dto.Password);

        var userId = await _userRepository.CreateUserAsync(dto, passwordHash, cancellationToken);

        return new RegisterResponseDto
        {
            UserId = userId,
            FullName = dto.FullName,
            Email = dto.Email
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email, cancellationToken);

        if (user == null || !user.IsActive)
            throw new AuthenticationException("Invalid email or password.");

        var passwordValid = _passwordHasher.Verify(dto.Password, user.PasswordHash);

        if (!passwordValid) throw new AuthenticationException("Invalid email or password.");

            var token = _jwtTokenGenerator.GenerateToken( user.UserId,  user.Email, user.FullName, user.Roles);
             var refreshToken = await _refreshTokenService.GenerateAsync( user.UserId, cancellationToken);

        return new LoginResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync( string refreshToken, CancellationToken cancellationToken = default)
     {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new BusinessException("Refresh token is required.");
            }

            // Validate the refresh token and get the associated UserId.
            var userId = await _refreshTokenService.ValidateAsync( refreshToken, cancellationToken);

            if (userId is null)
            {
                throw new AuthenticationException( "Invalid or expired refresh token.");
            }

            //  Get the current user and their roles.
            var user = await _userRepository.GetUserByIdAsync( userId.Value, cancellationToken);

            if (user is null || !user.IsActive)
            {
                throw new AuthenticationException(
                    "User associated with refresh token was not found or is inactive.");
            }

            //  Generate a new access token using the current user information and roles.
            var accessToken = _jwtTokenGenerator.GenerateToken(
                user.UserId,
                user.Email,
                user.FullName,
                user.Roles);

            //  Revoke the old refresh token.
            await _refreshTokenService.RevokeAsync( refreshToken, cancellationToken);

            // Generate and store a new refresh token.
            var newRefreshToken = await _refreshTokenService.GenerateAsync( user.UserId,cancellationToken);

            //  Return the new token pair.
            return new RefreshTokenResponseDto
            {
                Token = accessToken,
                RefreshToken = newRefreshToken
            };
    }
}