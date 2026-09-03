using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models;

public class ProductPrice
{
    public int ProductPriceId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Price { get; set; }
}