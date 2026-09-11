using AutoMapper;
using EcommerceAPI.DTOs.ProductPrices;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class ProductPriceService : IProductPriceService
{
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductPriceService> _logger;

    public ProductPriceService(IProductPriceRepository productPriceRepository, IMapper mapper, ILogger<ProductPriceService> logger)
    {
        _productPriceRepository = productPriceRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductPriceDto>> GetAllProductPricesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all product prices.");
        var productPrices = await _productPriceRepository.GetAllProductPricesAsync(cancellationToken);
        _logger.LogInformation("Fetched {Count} product prices.", productPrices.Count());
        return _mapper.Map<IEnumerable<ProductPriceDto>>(productPrices);
    }

    public async Task<ProductPriceDto?> GetProductPriceByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching product price for Product ID {ProductId}.", productId);
        var productPrice = await _productPriceRepository.GetProductPriceByProductIdAsync(productId, cancellationToken);
        return _mapper.Map<ProductPriceDto?>(productPrice);
    }
}
