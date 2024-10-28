namespace MTOGO.Services.ShoppingCartAPI.Models
{
    public class CartItem
    {
        public string MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
