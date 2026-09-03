namespace EcommerceAPI.DTOs.Products;

public class BatchStockRequestDto
{
    public List<BatchStockCheckItemDto> Items { get; set; } = new();
}
