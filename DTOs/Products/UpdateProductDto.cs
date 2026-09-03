using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Products;
public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
    public int StockQuantity { get; set; }
}