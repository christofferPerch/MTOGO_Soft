using Microsoft.AspNetCore.Mvc;
using MTOGO.Services.PaymentAPI.Models.Dto;
using MTOGO.Services.PaymentAPI.Services;
using Microsoft.Extensions.Logging;
using MTOGO.Services.PaymentAPI.Services.IServices;

namespace MTOGO.Services.PaymentAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentAPIController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentAPIController> _logger;
        protected ResponseDto _response;

        public PaymentAPIController(IPaymentService paymentService, ILogger<PaymentAPIController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
            _response = new ResponseDto();
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            try
            {
                if (paymentRequest == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Payment request is invalid.";
                    return BadRequest(_response);
                }

                await _paymentService.ProcessPayment(paymentRequest);
                _response.Message = "Payment processing initiated.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment for user {UserId}", paymentRequest.UserId);
                _response.IsSuccess = false;
                _response.Message = "An error occurred while processing the payment.";
                return StatusCode(500, _response);
            }
        }
    }
}
