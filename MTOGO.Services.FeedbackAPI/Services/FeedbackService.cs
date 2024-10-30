using MTOGO.Services.FeedbackAPI.Models;
using MTOGO.Services.FeedbackAPI.Models.Dto;
using MTOGO.Services.FeedbackAPI.Services.IServices;
using MTOGO.Services.DataAccess;
using Microsoft.Extensions.Logging;

namespace MTOGO.Services.FeedbackAPI.Services {
    public class FeedbackService : IFeedbackService {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(IDataAccess dataAccess, ILogger<FeedbackService> logger) {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task<int> AddDeliveryFeedbackAsync(DeliveryFeedbackDto deliveryFeedbackDto) {
            try {
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
    }
}
