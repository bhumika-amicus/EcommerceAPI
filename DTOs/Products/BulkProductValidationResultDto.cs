namespace EcommerceAPI.DTOs.Products;

public class BulkProductValidationResultDto
{
    public int RowNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsValid { get; set; }

    public List<string> Errors { get; set; } = new();
}