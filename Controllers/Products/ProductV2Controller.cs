using Asp.Versioning;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Attributes;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Products;

[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/products")]
[ApiController]
public class ProductV2Controller : ControllerBase
{
    private readonly IProductService _productService;

    public ProductV2Controller(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/v2/products/{productId}
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(
        int productId,
        CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Product ID must be greater than 0.",
                Data = null
            });
        }

        var product = await _productService.GetProductByIdAsync(
            productId,
            cancellationToken);

        if (product == null)
        {
            return NotFound(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Product not found.",
                Data = null
            });
        }

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Product retrieved successfully.",
            data = product
        });
    }


    // GET: api/v2/products
    [HttpGet]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ProductQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetAllProductsAsync(
            query,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Products retrieved successfully.",
            data = result
        });
    }


    // POST: api/v2/products
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [AuditLog("PRODUCT_CREATE", "Products")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        var createdProduct = await _productService.CreateProductAsync(
            dto,
            cancellationToken);

        if (createdProduct == null)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Failed to create product."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { productId = createdProduct.ProductId },
            new
            {
                apiVersion = "2.0",
                success = true,
                message = "Product created successfully.",
                data = createdProduct
            });
    }


    // PUT: api/v2/products/{productId}
    [HttpPut("{productId:int}")]
    [Authorize(Roles = "Admin")]
    [AuditLog("PRODUCT_UPDATE", "Products")]
    public async Task<IActionResult> Update(
        int productId,
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product ID must be greater than 0."
            });
        }

        var isUpdated = await _productService.UpdateProductAsync(
            productId,
            dto,
            cancellationToken);

        if (!isUpdated)
        {
            return NotFound(new
            {
                apiVersion = "2.0",
                success = false,
                message = $"Product with ID {productId} not found."
            });
        }

        var updatedProduct = await _productService.GetProductByIdAsync(
            productId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = $"Product with ID {productId} updated successfully.",
            data = updatedProduct
        });
    }


    // DELETE: api/v2/products/{productId}
    [HttpDelete("{productId:int}")]
    [Authorize(Roles = "Admin")]
    [AuditLog("PRODUCT_DELETE", "Products")]
    public async Task<IActionResult> Delete(
        int productId,
        CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product ID must be greater than 0."
            });
        }

        var isDeleted = await _productService.DeleteProductAsync(
            productId,
            cancellationToken);

        if (!isDeleted)
        {
            return NotFound(new
            {
                apiVersion = "2.0",
                success = false,
                message = $"Product with ID {productId} not found."
            });
        }

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = $"Product with ID {productId} deleted successfully.",
            data = productId
        });
    }


    // GET: api/v2/products/{productId}/availability
    [HttpGet("{productId:int}/availability")]
    public async Task<IActionResult> GetAvailability(
        int productId,
        CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product ID must be greater than 0."
            });
        }

        var availability = await _productService.GetProductAvailabilityAsync(
            productId,
            cancellationToken);

        if (availability == null)
        {
            return NotFound(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product not found."
            });
        }

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Product availability retrieved successfully.",
            data = availability
        });
    }


    // POST: api/v2/products/availability/batch
    [HttpPost("availability/batch")]
    public async Task<IActionResult> BatchCheckAvailability(
        [FromBody] BatchStockRequestDto request,
        CancellationToken cancellationToken)
    {
        var results = await _productService.ValidateBatchStockAsync(
            request.Items,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Batch stock validation completed.",
            data = results
        });
    }


    // POST: api/v2/products/{id}/image
    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadImage(
        int id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var isUploaded = await _productService.UploadProductImageAsync(
            id,
            file,
            cancellationToken);

        if (!isUploaded)
        {
            return NotFound(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product not found."
            });
        }

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Image uploaded successfully."
        });
    }


    // GET: api/v2/products/{id}/image
    [HttpGet("{id}/image")]
    [ResponseCache(
        Duration = 86400,
        Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> DownloadImage(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.DownloadProductImageAsync(
            id,
            cancellationToken);

        if (result == null)
        {
            return NotFound(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product image not found."
            });
        }

        return File(
            result.Value.FileStream,
            result.Value.ContentType,
            result.Value.FileName);
    }
}