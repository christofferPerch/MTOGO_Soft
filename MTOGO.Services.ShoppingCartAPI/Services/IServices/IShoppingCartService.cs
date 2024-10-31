using MTOGO.Services.ShoppingCartAPI.Models;

namespace MTOGO.Services.ShoppingCartAPI.Services.IServices
{
    public interface IShoppingCartService
    {
        Task<Cart?> CreateCart(Cart cart);
        Task<Cart?> GetCart(string userId);
        Task<Cart> UpdateCart(Cart cart);
        Task<bool> RemoveCart(string userId);
    }
}
