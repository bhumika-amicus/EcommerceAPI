namespace EcommerceAPI.DTOs.Orders;

public class OrderQueryDto
{
    public string? Search { get; set; }

    public string? OrderStatus { get; set; }

    public decimal? MinTotalAmount { get; set; }

    public decimal? MaxTotalAmount { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string SortBy { get; set; } = "orderid";

    public string SortDirection { get; set; } = "desc";

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
