using MTOGO.Services.FeedbackAPI.Models;
using MTOGO.Services.FeedbackAPI.Models.Dto;
using MTOGO.Services.FeedbackAPI.Services.IServices;
using MTOGO.Services.DataAccess;
using Microsoft.Extensions.Logging;
using MTOGO.MessageBus;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Models;


namespace MTOGO.Services.FeedbackAPI.Services {
    public class FeedbackService : IFeedbackService {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<FeedbackService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly IDistributedCache _distributedCache;
        private const string OrderStatusQueue = "OrderStatusQueue";

        public FeedbackService(IDataAccess dataAccess, IMessageBus messageBus, IDistributedCache distributedCache, ILogger<FeedbackService> logger) {
            _dataAccess = dataAccess;
            _messageBus = messageBus;
            _distributedCache = distributedCache;
            _logger = logger;

            // Subscribe to OrderStatusQueue for receiving updates
            SubscribeToOrderStatusQueue();
        }

        private void SubscribeToOrderStatusQueue() {
            _messageBus.SubscribeMessage<OrderStatusUpdateDto>(OrderStatusQueue, async (statusUpdate) => {
                _logger.LogInformation($"Received order status update for OrderId {statusUpdate.OrderId}, StatusId: {statusUpdate.StatusId}");

                // Cache the order status
                var cacheKey = GetOrderStatusCacheKey(statusUpdate.OrderId);
                await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(statusUpdate.StatusId), new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Set a suitable expiration time
                });
            });
        }

        public async Task<int> AddDeliveryFeedbackAsync(DeliveryFeedbackDto deliveryFeedbackDto) {
            try {
                // Check order status from cache
                var cacheKey = GetOrderStatusCacheKey(deliveryFeedbackDto.OrderId);
                var orderStatusJson = await _distributedCache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(orderStatusJson) || JsonConvert.DeserializeObject<int>(orderStatusJson) != (int)OrderStatus.Delivered) {
                    throw new InvalidOperationException("Cannot submit feedback for unconfirmed orders.");
                }

                // Insert feedback if order is confirmed
                var sql = @"
                    INSERT INTO DeliveryFeedback (OrderId, CustomerId, DeliveryAgentId, DeliveryExperienceRating, Comments, FeedbackTimestamp)
                    VALUES (@OrderId, @CustomerId, @DeliveryAgentId, @DeliveryExperienceRating, @Comments, @FeedbackTimestamp);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new {
                    deliveryFeedbackDto.OrderId,
                    deliveryFeedbackDto.CustomerId,
                    deliveryFeedbackDto.DeliveryAgentId,
                    deliveryFeedbackDto.DeliveryExperienceRating,
                    deliveryFeedbackDto.Comments,
                    FeedbackTimestamp = DateTime.Now
                };

                var id = await _dataAccess.InsertAndGetId<int>(sql, parameters);
                return id;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding delivery feedback.");
                throw;
            }
        }

        public async Task<int> AddRestaurantFeedbackAsync(RestaurantFeedbackDto restaurantFeedbackDto) {
            try {
                var sql = @"
                    INSERT INTO RestaurantFeedback (CustomerId, FoodRating, Comments, FeedbackTimestamp)
                    VALUES (@CustomerId, @FoodRating, @Comments, @FeedbackTimestamp);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new {
                    restaurantFeedbackDto.CustomerId,
                    restaurantFeedbackDto.FoodRating,
                    restaurantFeedbackDto.Comments,
                    FeedbackTimestamp = DateTime.Now
                };

                var id = await _dataAccess.InsertAndGetId<int>(sql, parameters);
                return id;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding restaurant feedback.");
                throw;
            }
        }

        public async Task<DeliveryFeedback?> GetDeliveryFeedbackAsync(int id) {
            var sql = "SELECT * FROM DeliveryFeedback WHERE Id = @Id;";
            return await _dataAccess.GetById<DeliveryFeedback>(sql, new { Id = id });
        }

        public async Task<RestaurantFeedback?> GetRestaurantFeedbackAsync(int id) {
            var sql = "SELECT * FROM RestaurantFeedback WHERE Id = @Id;";
            return await _dataAccess.GetById<RestaurantFeedback>(sql, new { Id = id });
        }

        private static string GetOrderStatusCacheKey(int orderId) => $"order_status:{orderId}";
    }
}
