using System;
using AutoMapper;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Validators;

namespace EcommerceAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    private readonly IMemoryCache _memoryCache;
    private const string ProductCacheVersionKey = "products_cache_version";

    private void InvalidateProductCache(int? productId = null)
    {
        var currentVersion = _memoryCache.GetOrCreate(ProductCacheVersionKey, entry => 1);

        _memoryCache.Set(ProductCacheVersionKey, currentVersion + 1);

        if (productId.HasValue)
        {
            _memoryCache.Remove($"product:{productId.Value}");
        }
    }

    public ProductService(IProductRepository productRepository, IMapper mapper, ILogger<ProductService> logger, IMemoryCache memoryCache)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{productId}";
        if (_memoryCache.TryGetValue(cacheKey, out ProductDto? cachedProduct))
        {
            return cachedProduct;
        }

        var productModel = await _productRepository.GetProductByIdAsync(productId, cancellationToken);
        var productDto = _mapper.Map<ProductDto?>(productModel);

        if (productDto != null)
        {
            _memoryCache.Set(cacheKey, productDto, TimeSpan.FromMinutes(10));
        }

        return productDto;
    }

    public async Task<PagedResult<ProductDto>> GetAllProductsAsync(ProductQueryDto query, CancellationToken cancellationToken = default)
    {

        var cacheVersion = _memoryCache.GetOrCreate(ProductCacheVersionKey, entry => 1);

        var cacheKey =
            $"products:v{cacheVersion}:" +
            $"{query.Search}:" +
            $"{query.CategoryId}:" +
            $"{query.BrandId}:" +
            $"{query.MinPrice}:" +
            $"{query.MaxPrice}:" +
            $"{query.MinRating}:" +
            $"{query.SortBy}:" +
            $"{query.SortDirection}:" +
            $"{query.PageNumber}:" +
            $"{query.PageSize}";

        if (_memoryCache.TryGetValue(cacheKey, out PagedResult<ProductDto>? cachedProducts))
        {
            return cachedProducts!;
        }

        var pagedModels = await _productRepository.GetAllProductsAsync(query, cancellationToken);

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(pagedModels.Items);

        var result = new PagedResult<ProductDto>
        {
            Items = productDtos,
            PageNumber = pagedModels.PageNumber,
            PageSize = pagedModels.PageSize,
            TotalCount = pagedModels.TotalCount
        };


        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;

    }

    public async Task<ProductDto?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var newProductId = await _productRepository.CreateProductAsync(dto, cancellationToken);
        InvalidateProductCache();
        _logger.LogInformation("Product Created: New ProductId {ProductId} created with Name {Name}", newProductId, dto.Name);
        return await GetProductByIdAsync(newProductId, cancellationToken);
    }

    public async Task<bool> UpdateProductAsync(int productId, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var isUpdated = await _productRepository.UpdateProductAsync(productId, dto, cancellationToken);
        if (isUpdated)
        {
            InvalidateProductCache(productId);
            _logger.LogInformation("Product Updated: ProductId {ProductId} updated successfully", productId);
        }
        else
        {
            _logger.LogWarning("Product Update Failed: ProductId {ProductId} not found", productId);
        }
        return isUpdated;
    }

    public async Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var isDeleted = await _productRepository.DeleteProductAsync(productId, cancellationToken);
        if (isDeleted)
        {
            InvalidateProductCache(productId);
            _logger.LogInformation("Product Deleted: ProductId {ProductId} deleted successfully", productId);
        }
        else
        {
            _logger.LogWarning("Product Delete Failed: ProductId {ProductId} not found", productId);
        }
        return isDeleted;
    }

    public async Task<ProductAvailabilityDto?> GetProductAvailabilityAsync(int productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking Stock Availability for ProductId {ProductId}", productId);
        return await _productRepository.GetProductAvailabilityAsync(productId, cancellationToken);
    }

    public async Task<IEnumerable<BatchStockValidationResultDto>> ValidateBatchStockAsync(IEnumerable<BatchStockCheckItemDto> items, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Batch Stock Validation for {Count} items", items.Count());
        return await _productRepository.ValidateBatchStockAsync(items, cancellationToken);
    }


    public async Task<bool> UploadProductImageAsync(int productId, IFormFile file, CancellationToken cancellationToken = default)
    {
        //  Check that the product exists
        var product = await _productRepository.GetProductByIdAsync(productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {productId} was not found.");
        }

        // Check that a file was actually provided
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("No file was uploaded.");
        }

        //  Allowed file extensions
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            throw new BusinessException("Only JPG, JPEG, PNG, and WEBP images are allowed.");
        }

        //  Maximum file size = 5 MB
        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            throw new BusinessException("File size cannot exceed 5 MB.");
        }

        // Actual file signature valid?
        var isValidImage = await ImageValidator.IsValidImageFileAsync(file, extension, cancellationToken);
        if (!isValidImage)
        {
            throw new BusinessException("The uploaded file is not a valid image.");
        }

        //  Create the image folder
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "product-images");
        Directory.CreateDirectory(folder);

        //  Generate a unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folder, fileName);

        try
        {
            //  Save the file to disk
            await using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await file.CopyToAsync(stream, cancellationToken);

            // Path that will be stored in SQL Server
            var imagePath = $"product-images/{fileName}";

            //  Save the path in the database
            var isUpdated = await _productRepository.UpdateImagePathAsync(productId, imagePath, cancellationToken);

            if (!isUpdated)
            {
                // Database update failed → remove the file we just created
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                throw new BusinessException("The image was uploaded, but the product image could not be updated.");
            }

            // Delete the OLD image from disk to prevent disk space leaks
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath);
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            //  Invalidate cached product data
            InvalidateProductCache(productId);

            _logger.LogInformation(
                "Product Image Updated: ProductId {ProductId} image path updated to {ImagePath}",
                productId,
                imagePath);

            return true;
        }
        catch
        {
            // If anything fails after the file was created, clean up the newly-created file.
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)?> DownloadProductImageAsync(int productId, CancellationToken cancellationToken = default)
    {
        // Get the product so we can get its ImagePath
        var product = await _productRepository.GetProductByIdAsync(productId, cancellationToken);

        // Product doesn't exist or doesn't have an image
        if (product == null || string.IsNullOrEmpty(product.ImagePath))
        {
            return null;
        }

        // Build the physical path of the image
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath);

        // Check whether the physical file actually exists
        if (!File.Exists(filePath))
        {
            return null;
        }

        // Determine the MIME type
        var contentType = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        // 6. Open the file as a stream
        var fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true
        );

        // 7. Get the file name
        var fileName = Path.GetFileName(filePath);

        // 8. Return the stream information
        return (fileStream, contentType, fileName);
    }

    public async Task<BulkCreateProductResponseDto> BulkCreateProductsAsync( List<CreateProductDto> products,  CancellationToken cancellationToken = default)
    {
        if (products == null || products.Count == 0)
        {
            throw new BusinessException(
                "At least one product is required.");
        }
        return await _productRepository.BulkCreateProductsAsync( products, cancellationToken);
    }

  
    public async Task<BulkInventoryUpdateResponseDto> BulkUpdateInventoryAsync( List<BulkInventoryUpdateItemDto> items,
        CancellationToken cancellationToken = default)
        {
            
            if (items == null || items.Count == 0)
            {
                throw new BusinessException(
                    "At least one inventory update item is required.");
            }

        var result = await _productRepository.BulkUpdateInventoryAsync( items, cancellationToken);

        foreach (var item in items)
        {
            InvalidateProductCache(item.ProductId);
        }

        return result;


    }


}
