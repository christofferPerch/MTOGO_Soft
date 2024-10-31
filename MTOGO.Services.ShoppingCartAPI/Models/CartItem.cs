namespace MTOGO.Services.ShoppingCartAPI.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
