using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models;

public class Brand
{
    public int BrandId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}