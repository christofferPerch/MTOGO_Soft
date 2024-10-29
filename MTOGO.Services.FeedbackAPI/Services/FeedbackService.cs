using Dapper;
using MTOGO.Services.DataAccess;
using MTOGO.Services.FeedbackAPI.Models.Dto;
using MTOGO.Services.FeedbackAPI.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MTOGO.Services.FeedbackAPI.Services {
    public class FeedbackService : IFeedbackService {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(IDataAccess dataAccess, ILogger<FeedbackService> logger) {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task<int> AddFeedbackAsync(FeedbackCreateDto feedbackDto) {
            try {
                if (!await CanSubmitFeedbackAsync(feedbackDto.OrderId)) {
                    _logger.LogWarning($"Cannot submit feedback for OrderId {feedbackDto.OrderId}. Order not delivered.");
                    return 0;
                }

                var sql = @"
                    INSERT INTO Feedback (OrderId, CustomerId, DeliveryAgentId, FoodRating, DeliveryExperienceRating, DeliveryAgentRating, Comments, FeedbackTimestamp)
                    VALUES (@OrderId, @CustomerId, @DeliveryAgentId, @FoodRating, @DeliveryExperienceRating, @DeliveryAgentRating, @Comments, @FeedbackTimestamp);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new {
                    feedbackDto.OrderId,
                    feedbackDto.CustomerId,
                    feedbackDto.DeliveryAgentId,
                    feedbackDto.FoodRating,
                    feedbackDto.DeliveryExperienceRating,
                    feedbackDto.DeliveryAgentRating,
                    feedbackDto.Comments,
                    FeedbackTimestamp = DateTime.UtcNow
                };

                return await _dataAccess.InsertAndGetId<int?>(sql, parameters) ?? 0;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding feedback");
                throw;
            }
        }

        public async Task<List<FeedbackDto>> GetFeedbackByOrderIdAsync(int orderId) {
            try {
                var sql = "SELECT * FROM Feedback WHERE OrderId = @OrderId;";
                return (await _dataAccess.GetAll<FeedbackDto>(sql, new { OrderId = orderId })).ToList();
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error retrieving feedback for OrderId {orderId}");
                throw;
            }
        }

        public async Task<List<FeedbackDto>> GetAllFeedbackAsync() {
            try {
                var sql = "SELECT * FROM Feedback;";
                return (await _dataAccess.GetAll<FeedbackDto>(sql)).ToList();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error retrieving all feedback");
                throw;
            }
        }

        public async Task<bool> CanSubmitFeedbackAsync(int orderId) {
            try {
                var sql = @"
            SELECT COUNT(1)
            FROM [Order]
            WHERE Id = @OrderId 
              AND OrderStatusId = (SELECT Id FROM OrderStatus WHERE Status = 'Delivered');";

                return (await _dataAccess.ExecuteScalarAsync<int>(sql, new { OrderId = orderId })) > 0;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error checking if feedback can be submitted for OrderId {orderId}");
                throw;
            }
        }
    }
}
