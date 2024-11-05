using Microsoft.AspNetCore.Mvc;
using MTOGO.Services.PaymentAPI.Models.Dto;
using MTOGO.Services.PaymentAPI.Services;

namespace MTOGO.Services.PaymentAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid payment data." });
            }

            var paymentResponse = await _paymentService.ProcessPayment(paymentRequest);
            return Ok(new ResponseDto { Result = paymentResponse, IsSuccess = paymentResponse.IsSuccessful, Message = paymentResponse.Message });
        }
    }
}
