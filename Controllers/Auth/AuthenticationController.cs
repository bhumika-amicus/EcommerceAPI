using EcommerceAPI.Common;
using EcommerceAPI.Common.Attributes;
using EcommerceAPI.DTOs.Authentication;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController( IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    [AuditLog("USER_REGISTER", "Users")]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register( [FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync( dto, cancellationToken);

        return Ok(new ApiResponse<RegisterResponseDto>
        {
            Success = true,
            Message = "User registered successfully.",
            Data = result
        });
    }

    [HttpPost("login")]
    [AuditLog("USER_LOGIN", "Users")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login( [FromBody] LoginDto dto, CancellationToken cancellationToken)
    {

        var result = await _authenticationService.LoginAsync( dto, cancellationToken);

        return Ok(new ApiResponse<LoginResponseDto>
        {
            Success = true,
            Message = "Login successful.",
            Data = result
        });
    }
}