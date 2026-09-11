
namespace EcommerceAPI.DTOs.Products;

public class BulkInventoryUpdateResponseDto
{
    public int TotalRecords { get; set; }

    public int SuccessfulRecords { get; set; }

    public int FailedRecords { get; set; }

    public List<BulkInventoryUpdateResultDto> Results { get; set; } = new();
}
