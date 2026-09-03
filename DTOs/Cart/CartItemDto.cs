namespace EcommerceAPI.DTOs.Cart;
public class CartItemDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPriceAtAddition { get; set; }
    public decimal CurrentPrice { get; set; }
    public int Quantity { get; set; }
    public decimal ItemSubtotal { get; set; } // Populated directly from SQL calculation!
    public bool HasPriceChanged => UnitPriceAtAddition != CurrentPrice;
    public string PriceChangeMessage => HasPriceChanged
        ? (CurrentPrice > UnitPriceAtAddition
            ? $"Price increased by {(CurrentPrice - UnitPriceAtAddition):C} since added to cart."
            : $"Price dropped by {(UnitPriceAtAddition - CurrentPrice):C} since added to cart!")
        : string.Empty;
}