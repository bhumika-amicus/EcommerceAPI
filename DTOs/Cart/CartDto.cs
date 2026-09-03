namespace EcommerceAPI.DTOs.Cart;
public class CartDto
{
    public int CustomerId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalItemCount => Items.Sum(i => i.Quantity);
    public decimal CartSubtotal => Items.Sum(i => i.ItemSubtotal);
}