using MTOGO.Services.FeedbackAPI.Models.Dto;
using MTOGO.Services.FeedbackAPI.Models;

namespace MTOGO.Services.FeedbackAPI.Services.IServices {
    public interface IFeedbackService {
        Task<int> AddDeliveryFeedbackAsync(DeliveryFeedbackDto deliveryFeedbackDto);
        Task<int> AddRestaurantFeedbackAsync(RestaurantFeedbackDto restaurantFeedbackDto);
        Task<DeliveryFeedback?> GetDeliveryFeedbackAsync(int id);
        Task<RestaurantFeedback?> GetRestaurantFeedbackAsync(int id);
    }
}
