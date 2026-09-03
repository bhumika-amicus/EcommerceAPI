namespace EcommerceAPI.DTOs.Products;

public class ProductQueryDto
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public decimal? MinRating { get; set; }

    public string SortBy { get; set; } = "name";

    public string SortDirection { get; set; } = "asc";

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}