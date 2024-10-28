using Microsoft.Extensions.Caching.Distributed;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;
using Newtonsoft.Json;

namespace MTOGO.Services.ShoppingCartAPI.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IDistributedCache _redisCache;

        public ShoppingCartService(IDistributedCache redisCache)
        {
            _redisCache = redisCache;
        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            var cartData = await _redisCache.GetStringAsync(userId);
            return string.IsNullOrEmpty(cartData) ? null : JsonConvert.DeserializeObject<Cart>(cartData);
        }

        public async Task<Cart> UpdateCartAsync(Cart cart)
        {
            await _redisCache.SetStringAsync(cart.UserId, JsonConvert.SerializeObject(cart));
            return await GetCartAsync(cart.UserId);
        }

        public async Task<bool> RemoveCartAsync(string userId)
        {
            await _redisCache.RemoveAsync(userId);
            return true;
        }
    }
}
