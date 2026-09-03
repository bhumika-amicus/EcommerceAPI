using AutoMapper;
using EcommerceAPI.DTOs.ProductPrices;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;

namespace EcommerceAPI.Services;

public class ProductPriceService : IProductPriceService
{
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IMapper _mapper;

    public ProductPriceService(IProductPriceRepository productPriceRepository, IMapper mapper)
    {
        _productPriceRepository = productPriceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductPriceDto>> GetAllProductPricesAsync(CancellationToken cancellationToken = default)
    {
        var productPrices = await _productPriceRepository.GetAllProductPricesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProductPriceDto>>(productPrices);
    }

    public async Task<ProductPriceDto?> GetProductPriceByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var productPrice = await _productPriceRepository.GetProductPriceByProductIdAsync(productId, cancellationToken);
        return _mapper.Map<ProductPriceDto?>(productPrice);
    }
}
