using EcommerceAPI.Common;
using EcommerceAPI.DTOs.ProductPrices;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.ProductPrices;

[Route("api/product-prices")]
[ApiController]
public class ProductPriceController : ControllerBase
{
    private readonly IProductPriceService _productPriceService;

    public ProductPriceController(IProductPriceService productPriceService)
    {
        _productPriceService = productPriceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductPriceDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var productPrices = await _productPriceService.GetAllProductPricesAsync(cancellationToken);

        return Ok(new ApiResponse<IEnumerable<ProductPriceDto>>
        {
            Success = true,
            Message = "Product prices retrieved successfully.",
            Data = productPrices
        });
    }

    [HttpGet("{productId:int}")]
    public async Task<ActionResult<ApiResponse<ProductPriceDto>>> GetByProductId(int productId, CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<ProductPriceDto>
            {
                Success = false,
                Message = "Product ID must be greater than 0."
            });
        }

        var productPrice = await _productPriceService.GetProductPriceByProductIdAsync(productId, cancellationToken);

        if (productPrice == null)
        {
            return NotFound(new ApiResponse<ProductPriceDto>
            {
                Success = false,
                Message = "Product price not found."
            });
        }

        return Ok(new ApiResponse<ProductPriceDto>
        {
            Success = true,
            Message = "Product price retrieved successfully.",
            Data = productPrice
        });
    }
}
