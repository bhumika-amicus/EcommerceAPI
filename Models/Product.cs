using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }

    [Required]
    [Range(0, 5)]
    public decimal Rating { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}