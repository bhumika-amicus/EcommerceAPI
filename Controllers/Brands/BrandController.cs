using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Brands;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Brands;

[Route("api/brands")]
[ApiController]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BrandDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var brands = await _brandService.GetAllBrandsAsync(cancellationToken);

        return Ok(new ApiResponse<IEnumerable<BrandDto>>
        {
            Success = true,
            Message = "Brands retrieved successfully.",
            Data = brands
        });
    }

    [HttpGet("{brandId:int}")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(int brandId, CancellationToken cancellationToken)
    {
        if (brandId <= 0)
        {
            return BadRequest(new ApiResponse<BrandDto>
            {
                Success = false,
                Message = "Brand ID must be greater than 0."
            });
        }

        var brand = await _brandService.GetBrandByIdAsync(brandId, cancellationToken);

        if (brand == null)
        {
            return NotFound(new ApiResponse<BrandDto>
            {
                Success = false,
                Message = "Brand not found."
            });
        }

        return Ok(new ApiResponse<BrandDto>
        {
            Success = true,
            Message = "Brand retrieved successfully.",
            Data = brand
        });
    }
}
