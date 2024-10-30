using MTOGO.Services.ShoppingCartAPI.Models;

namespace MTOGO.Services.ShoppingCartAPI.Services.IServices
{
    public interface IShoppingCartService
    {
        Task<Cart?> CreateCartAsync(Cart cart);
        Task<Cart?> GetCartAsync(string userId);
        Task<Cart> UpdateCartAsync(Cart cart);
        Task<bool> RemoveCartAsync(string userId);
    }
}
