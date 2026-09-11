namespace EcommerceAPI.DTOs.Products;

public class BulkCreateProductResponseDto
{
    public int TotalRecords { get; set; }

    public int SuccessfulRecords { get; set; }

    public int FailedRecords { get; set; }

    public List<BulkProductValidationResultDto> Results { get; set; } = new();
}