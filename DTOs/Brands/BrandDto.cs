namespace EcommerceAPI.DTOs.Brands;

public class BrandDto
{
    public int BrandId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}