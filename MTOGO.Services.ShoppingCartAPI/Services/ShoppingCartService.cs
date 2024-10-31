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

        private readonly string _cartCreatedQueue;
        private readonly string _cartUpdatedQueue;
        private readonly string _cartRemovedQueue;
        private readonly string _cartRequestQueue;
        private readonly string _cartResponseQueue;

        public ShoppingCartService(IDistributedCache redisCache, IMessageBus messageBus,
            ILogger<ShoppingCartService> logger, IConfiguration configuration)
        {
            _redisCache = redisCache;
            _messageBus = messageBus;
            _logger = logger;
            _configuration = configuration;

            _cartCreatedQueue = _configuration.GetValue<string>("TopicAndQueueNames:CartCreatedQueue");
            _cartUpdatedQueue = _configuration.GetValue<string>("TopicAndQueueNames:CartUpdatedQueue");
            _cartRemovedQueue = _configuration.GetValue<string>("TopicAndQueueNames:CartRemovedQueue");
            _cartRequestQueue = _configuration.GetValue<string>("TopicAndQueueNames:CartRequestQueue");
            _cartResponseQueue = _configuration.GetValue<string>("TopicAndQueueNames:CartResponseQueue");

            if (string.IsNullOrEmpty(_cartCreatedQueue) || string.IsNullOrEmpty(_cartUpdatedQueue) ||
                string.IsNullOrEmpty(_cartRemovedQueue) || string.IsNullOrEmpty(_cartRequestQueue) ||
                string.IsNullOrEmpty(_cartResponseQueue))
            {
                throw new Exception("One or more queue names are not configured properly in appsettings.json.");
            }

            SubscribeToCartRequestQueue();
        }

        private void SubscribeToCartRequestQueue()
        {
            _messageBus.SubscribeMessage<CartRequestMessageDto>(_cartRequestQueue, async (cartRequest) =>
            {
                _logger.LogInformation($"Received cart request message for user {cartRequest.UserId} with CorrelationId: {cartRequest.CorrelationId}");
                await ProcessCartRequest(cartRequest);
            });
        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            var cartData = await _redisCache.GetStringAsync(userId);
            return string.IsNullOrEmpty(cartData) ? null : JsonConvert.DeserializeObject<Cart>(cartData);
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            var existingCart = await GetCartAsync(cart.UserId);
            if (existingCart != null)
            {
                throw new InvalidOperationException($"Cart already exists for user {cart.UserId}");
            }

            await _redisCache.SetStringAsync(cart.UserId, JsonConvert.SerializeObject(cart));
            await _messageBus.PublishMessage(_cartCreatedQueue, JsonConvert.SerializeObject(cart));

            _logger.LogInformation($"Created new cart for user {cart.UserId}");
            return cart;
        }

        public async Task<Cart> UpdateCartAsync(Cart cart)
        {
            await _redisCache.SetStringAsync(cart.UserId, JsonConvert.SerializeObject(cart));
            await _messageBus.PublishMessage(_cartUpdatedQueue, JsonConvert.SerializeObject(cart));

            _logger.LogInformation($"Updated cart for user {cart.UserId}");
            return await GetCartAsync(cart.UserId);
        }

        public async Task<bool> RemoveCartAsync(string userId)
        {
            await _redisCache.RemoveAsync(userId);
            await _messageBus.PublishMessage(_cartRemovedQueue, $"Cart for user {userId} removed");

            _logger.LogInformation($"Removed cart for user {userId}");
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

            _logger.LogInformation($"Publishing cart response with CorrelationId: {cartResponse.CorrelationId} to CartResponseQueue");
            await _messageBus.PublishMessage("CartResponseQueue", JsonConvert.SerializeObject(cartResponse));
        }


    }
}
