
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Attributes;
using EcommerceAPI.DTOs.Addresses;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Addresses;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/address")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(claim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException( "User ID claim is missing or invalid in token.");
        }
    }

    // GET: api/v1/address
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AddressDto>>> GetAddress( CancellationToken cancellationToken = default)
    {
        var address = await _addressService.GetAddressByUserIdAsync( CurrentUserId, cancellationToken);

        if (address == null)
        {
            return NotFound(new ApiResponse<AddressDto>
            {
                Success = false,
                Message = "Address not found."
            });
        }

        return Ok(new ApiResponse<AddressDto>
        {
            Success = true,
            Message = "Address retrieved successfully.",
            Data = address
        });
    }

    // PUT: api/v1/address
    [HttpPut]
    [AuditLog("ADDRESS_UPDATE", "Addresses")]
    public async Task<ActionResult<ApiResponse<AddressDto>>> SaveAddress( [FromBody] AddressReqDto dto, CancellationToken cancellationToken = default)
    {
        var address = await _addressService.SaveAddressAsync( CurrentUserId,  dto,  cancellationToken);

        return Ok(new ApiResponse<AddressDto>
        {
            Success = true,
            Message = "Address saved successfully.",
            Data = address
        });
    }

    // DELETE: api/v1/address
    [HttpDelete]
    [AuditLog("ADDRESS_DELETE", "Addresses")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAddress( CancellationToken cancellationToken = default)
    {
        var isDeleted = await _addressService.DeleteAddressAsync( CurrentUserId, cancellationToken);

        if (!isDeleted)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Address not found."
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Address deleted successfully."
        });
    }
}

