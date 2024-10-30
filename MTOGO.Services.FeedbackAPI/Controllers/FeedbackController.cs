using MTOGO.Services.FeedbackAPI.Models.Dto;
using MTOGO.Services.FeedbackAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MTOGO.Services.FeedbackAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService) {
            _feedbackService = feedbackService;
        }

        [HttpPost("delivery")]
        public async Task<IActionResult> AddDeliveryFeedback([FromBody] DeliveryFeedbackDto deliveryFeedbackDto) {
            var id = await _feedbackService.AddDeliveryFeedbackAsync(deliveryFeedbackDto);
            return Ok(new { Message = "Delivery feedback added successfully.", FeedbackId = id });
        }

        [HttpPost("restaurant")]
        public async Task<IActionResult> AddRestaurantFeedback([FromBody] RestaurantFeedbackDto restaurantFeedbackDto) {
            var id = await _feedbackService.AddRestaurantFeedbackAsync(restaurantFeedbackDto);
            return Ok(new { Message = "Restaurant feedback added successfully.", FeedbackId = id });
        }

        [HttpGet("delivery/{id}")]
        public async Task<IActionResult> GetDeliveryFeedback(int id) {
            var feedback = await _feedbackService.GetDeliveryFeedbackAsync(id);
            return feedback == null ? NotFound() : Ok(feedback);
        }

        [HttpGet("restaurant/{id}")]
        public async Task<IActionResult> GetRestaurantFeedback(int id) {
            var feedback = await _feedbackService.GetRestaurantFeedbackAsync(id);
            return feedback == null ? NotFound() : Ok(feedback);
        }
    }
}
