namespace EcommerceAPI.DTOs.Products
{
    public class BulkInventoryUpdateRequestDto {
        public List<BulkInventoryUpdateItemDto> Items { get; set; } = new(); 
    }
}
