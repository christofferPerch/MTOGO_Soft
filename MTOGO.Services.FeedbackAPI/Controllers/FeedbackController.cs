using MTOGO.Services.FeedbackAPI.Models.Dto;
using MTOGO.Services.FeedbackAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MTOGO.Services.FeedbackAPI.Controllers {
    [Route("api/feedback")]
    [ApiController]
    public class FeedbackController : ControllerBase {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;
        protected ResponseDto _response;

        public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger) {
            _feedbackService = feedbackService;
            _logger = logger;
            _response = new();
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackCreateDto feedbackDto) {
            _logger.LogInformation("Received request to submit feedback.");

            try {
                if (feedbackDto == null) {
                    _response.IsSuccess = false;
                    _response.Message = "Feedback data is invalid.";
                    return BadRequest(_response);
                }

                var feedbackId = await _feedbackService.AddFeedbackAsync(feedbackDto);
                if (feedbackId == 0) {
                    _response.IsSuccess = false;
                    _response.Message = "Cannot submit feedback. Order might not be delivered or an error occurred.";
                    return BadRequest(_response);
                }

                _response.IsSuccess = true;
                _response.Message = "Feedback submitted successfully.";
                _response.Result = feedbackId;

                return Ok(_response);
            } catch (Exception ex) {
                _logger.LogError(ex, "An error occurred while submitting feedback.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while submitting feedback.";
                return StatusCode(500, _response);
            }
        }

        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetFeedbackByOrderId(int orderId) {
            _logger.LogInformation($"Received request to retrieve feedback for OrderId {orderId}.");

            try {
                var feedbackList = await _feedbackService.GetFeedbackByOrderIdAsync(orderId);
                if (feedbackList == null || feedbackList.Count == 0) {
                    _response.IsSuccess = false;
                    _response.Message = "No feedback found for this order.";
                    return NotFound(_response);
                }

                _response.IsSuccess = true;
                _response.Message = "Feedback retrieved successfully.";
                _response.Result = feedbackList;
                return Ok(_response);
            } catch (Exception ex) {
                _logger.LogError(ex, $"An error occurred while retrieving feedback for OrderId {orderId}.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving feedback.";
                return StatusCode(500, _response);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFeedback() {
            _logger.LogInformation("Received request to retrieve all feedback.");

            try {
                var feedbackList = await _feedbackService.GetAllFeedbackAsync();
                _response.IsSuccess = true;
                _response.Message = "All feedback retrieved successfully.";
                _response.Result = feedbackList;
                return Ok(_response);
            } catch (Exception ex) {
                _logger.LogError(ex, "An error occurred while retrieving all feedback.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving all feedback.";
                return StatusCode(500, _response);
            }
        }
    }
}
