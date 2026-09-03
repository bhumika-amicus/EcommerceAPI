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

    public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
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

        return new LoginResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Token = token
        };
    }
}