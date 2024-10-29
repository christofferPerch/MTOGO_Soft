using MTOGO.Services.FeedbackAPI.Models.Dto;

namespace MTOGO.Services.FeedbackAPI.Services.IServices {
    public interface IFeedbackService {
        Task<int> AddFeedbackAsync(FeedbackCreateDto feedbackDto);
        Task<List<FeedbackDto>> GetFeedbackByOrderIdAsync(int orderId);
        Task<List<FeedbackDto>> GetAllFeedbackAsync();
        Task<bool> CanSubmitFeedbackAsync(int orderId);
    }

}
