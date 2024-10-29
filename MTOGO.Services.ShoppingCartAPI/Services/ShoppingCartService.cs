using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;
using Newtonsoft.Json;

namespace MTOGO.Services.ShoppingCartAPI.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IDistributedCache _redisCache;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IConfiguration _configuration;


        public ShoppingCartService(IDistributedCache redisCache, IMessageBus messageBus, 
            ILogger<ShoppingCartService> logger, IConfiguration configuration)
        {
            _redisCache = redisCache;
            _messageBus = messageBus;
            _logger = logger;
            _configuration = configuration;

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

        public async Task ProcessCartRequest(CartRequestMessageDto cartRequest)
        {
            var cart = await GetCartAsync(cartRequest.UserId);
            if (cart == null)
            {
                _logger.LogWarning($"No cart found for user {cartRequest.UserId}");
                return;
            }

            var cartResponse = new CartResponseMessageDto
            {
                UserId = cartRequest.UserId,
                CorrelationId = cartRequest.CorrelationId,
                Items = cart.Items.Select(item => new OrderItemDto
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            _logger.LogInformation($"Publishing cart response with CorrelationId: {cartResponse.CorrelationId} to {_configuration["TopicAndQueueNames:CartResponseQueue"]}");
            await _messageBus.PublishMessage(_configuration["TopicAndQueueNames:CartResponseQueue"], JsonConvert.SerializeObject(cartResponse));
        }

    }
}
