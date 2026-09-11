namespace EcommerceAPI.DTOs.Products
{
    public class BulkInventoryUpdateResultDto
    {
        public int RowNumber { get; set; }
        public int ProductId { get; set; }
        public int StockQuantity { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
