namespace BlazeStore.Client.Models;

public record Cart(IReadOnlyList<CartItem> Items)
{
    public int TotalCount => Items.Sum(i => i.Quantity);
    public decimal TotalPrice => Items.Sum(i => i.Product.Price * i.Quantity);
}
