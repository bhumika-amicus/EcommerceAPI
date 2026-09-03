namespace EcommerceAPI.DTOs.ProductPrices;

public class ProductPriceDto
{
    public int ProductPriceId { get; set; }

    public int ProductId { get; set; }

    public decimal Price { get; set; }
}