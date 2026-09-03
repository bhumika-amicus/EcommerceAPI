namespace EcommerceAPI.DTOs.Products
{
    public class BatchStockCheckItemDto
    {
        public int ProductId { get; set; }
        public int RequestedQuantity { get; set; }
    }

    public class BatchStockValidationResultDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}
