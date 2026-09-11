namespace EcommerceAPI.DTOs.Products;

public class BulkCreateProductRequestDto
{
    public List<CreateProductDto> Products { get; set; } = new();
}