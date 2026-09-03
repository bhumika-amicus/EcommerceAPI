using AutoMapper;
using EcommerceAPI.DTOs.Brands;
using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.DTOs.ProductPrices;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Models;

namespace EcommerceAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<Brand, BrandDto>();
        CreateMap<ProductPrice, ProductPriceDto>();
        CreateMap<ProductDetailModel, ProductDto>();
    }
}
