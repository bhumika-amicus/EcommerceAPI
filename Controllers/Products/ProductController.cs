using Asp.Versioning;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Attributes;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace EcommerceAPI.Controllers.Products;


[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/products")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products/{productId}
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int productId, CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Product ID must be greater than 0."
            });
        }
     // Fetch the product from the database (Normal behavior)
        var product = await _productService.GetProductByIdAsync(productId, cancellationToken);

        if (product == null)
        {
            return NotFound(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Product not found."
            });
        }

    //generate the etag
        var etag = GenerateETag(product);

    //Check if the client sent an "If-None-Match" header and its matching our tag then return 304 not modified
        if (Request.Headers.TryGetValue("If-None-Match", out var clientETag) && clientETag == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

    //add the etag to the response header if client Etags doesnt match
        Response.Headers.ETag = etag;

        return Ok(new ApiResponse<ProductDto>
        {
            Success = true,
            Message = "Product retrieved successfully.",
            Data = product
        });
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetAll([FromQuery] ProductQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _productService.GetAllProductsAsync(query, cancellationToken);

        var etag = GenerateETag(result);

        if (Request.Headers.TryGetValue("If-None-Match", out var clientETag) && clientETag == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;

        return Ok(new ApiResponse<PagedResult<ProductDto>>
        {
            Success = true,
            Message = "Products retrieved successfully.",
            Data = result
        });
    }

    // POST: api/products (Create Product)
    [HttpPost]
    [Authorize(Policy = "CanManageProducts")]
    [AuditLog("PRODUCT_CREATE", "Products")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var createdProduct = await _productService.CreateProductAsync(dto, cancellationToken);
        if (createdProduct == null)
        {
            return BadRequest(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Failed to create product."
            });
        }
        return CreatedAtAction(
            nameof(GetById),
            new { productId = createdProduct.ProductId },
            new ApiResponse<ProductDto>
            {
                Success = true,
                Message = "Product created successfully.",
                Data = createdProduct
            });
    }

    // PUT: api/products/{productId} (Update Product)
    [HttpPut("{productId:int}")]
    [Authorize(Policy = "CanManageProducts")]
    [AuditLog("PRODUCT_UPDATE", "Products")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int productId, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Product ID must be greater than 0."
            });
        }

        var isUpdated = await _productService.UpdateProductAsync(productId, dto, cancellationToken);
        if (!isUpdated)
        {
            return NotFound(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = $"Product with ID {productId} not found."
            });
        }

        var updatedProduct = await _productService.GetProductByIdAsync(productId, cancellationToken);

        return Ok(new ApiResponse<ProductDto>
        {
            Success = true,
            Message = $"Product with ID {productId} updated successfully.",
            Data = updatedProduct
        });
    }

    // DELETE: api/products/{productId} (Delete Product)
    [HttpDelete("{productId:int}")]
    [Authorize(Policy = "CanManageProducts")]
    [AuditLog("PRODUCT_DELETE", "Products")]
    public async Task<ActionResult<ApiResponse<int>>> Delete(int productId, CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<int>
            {
                Success = false,
                Message = "Product ID must be greater than 0."
            });
        }

        var isDeleted = await _productService.DeleteProductAsync(productId, cancellationToken);
        if (!isDeleted)
        {
            return NotFound(new ApiResponse<int>
            {
                Success = false,
                Message = $"Product with ID {productId} not found."
            });
        }

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Message = $"Product with ID {productId} deleted successfully.",
            Data = productId
        });
    }

    // GET: api/products/{productId}/availability
    [HttpGet("{productId:int}/availability")]
    public async Task<ActionResult<ApiResponse<ProductAvailabilityDto>>> GetAvailability(int productId, CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<ProductAvailabilityDto>
            {
                Success = false,
                Message = "Product ID must be greater than 0."
            });
        }

        var availability = await _productService.GetProductAvailabilityAsync(productId, cancellationToken);
        if (availability == null)
        {
            return NotFound(new ApiResponse<ProductAvailabilityDto>
            {
                Success = false,
                Message = "Product not found."
            });
        }

        var etag = GenerateETag(availability);

        if (Request.Headers.TryGetValue("If-None-Match", out var clientETag) && clientETag == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;

        return Ok(new ApiResponse<ProductAvailabilityDto>
        {
            Success = true,
            Message = "Product availability retrieved successfully.",
            Data = availability
        });
    }

    // POST: api/products/availability/batch
    [HttpPost("availability/batch")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BatchStockValidationResultDto>>>> BatchCheckAvailability([FromBody] BatchStockRequestDto request, CancellationToken cancellationToken)
    {
        var results = await _productService.ValidateBatchStockAsync(request.Items, cancellationToken);
        return Ok(new ApiResponse<IEnumerable<BatchStockValidationResultDto>>
        {
            Success = true,
            Message = "Batch stock validation completed.",
            Data = results
        });
    }

    // Upload Product Image
    [HttpPost("{id}/image")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> UploadImage( int id, IFormFile file, CancellationToken cancellationToken) {

        var isUploaded = await _productService.UploadProductImageAsync( id, file, cancellationToken); 
        if (!isUploaded) {
            return NotFound(new { Message = "Product not found." });
        }
        return Ok(new { Message = "Image uploaded successfully." }); 
    }

    [HttpGet("{id}/image")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> DownloadImage( int id, CancellationToken cancellationToken)
    {
        var result = await _productService.DownloadProductImageAsync( id, cancellationToken);

        if (result == null)
        {
            return NotFound(new
            {
                Message = "Product image not found."
            });
        }

        return File(
            result.Value.FileStream,
            result.Value.ContentType,
            result.Value.FileName);
    }

    
    [HttpPost("bulk")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<ActionResult<BulkCreateProductResponseDto>> BulkCreateProducts( [FromBody] BulkCreateProductRequestDto request,
        CancellationToken cancellationToken)
        {
            var result = await _productService.BulkCreateProductsAsync( request.Products,
                cancellationToken);

            return Ok(result);
    }


    [HttpPut("bulk/inventory")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> BulkUpdateInventory( [FromBody] BulkInventoryUpdateRequestDto request,
        CancellationToken cancellationToken)
        {
            var result = await _productService.BulkUpdateInventoryAsync(
                request.Items,
                cancellationToken);

            return Ok(new ApiResponse<BulkInventoryUpdateResponseDto>
            {
                Success = true,
                Message = "Bulk inventory update completed.",
                Data = result
            });
        }

    private string GenerateETag(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = MD5.HashData(bytes);
        return $"\"{Convert.ToBase64String(hash)}\"";
    }
}