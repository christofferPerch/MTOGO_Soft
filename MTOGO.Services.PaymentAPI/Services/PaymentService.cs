using MTOGO.MessageBus;
using MTOGO.Services.PaymentAPI.Models.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTOGO.Services.PaymentAPI.Services.IServices;

namespace MTOGO.Services.PaymentAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<PaymentService> _logger;
        private readonly string _paymentResponseQueue;

        public PaymentService(IMessageBus messageBus, IConfiguration configuration, ILogger<PaymentService> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
            _paymentResponseQueue = configuration["RabbitMQ:TopicAndQueueNames:PaymentResponseQueue"];
        }

        public async Task ProcessPayment(PaymentRequestDto paymentRequest)
        {
            if (!ValidatePaymentDetails(paymentRequest))
            {
                PublishPaymentResponse(paymentRequest, false, "Invalid payment details.");
                return;
            }

            // Simulate payment processing delay
            await Task.Delay(2000); // 2 seconds delay for realism

            bool isSuccess = new Random().Next(0, 2) == 0; // Random success/failure for testing
            string message = isSuccess ? "Payment processed successfully." : "Payment processing failed.";

            PublishPaymentResponse(paymentRequest, isSuccess, message);
        }

        private bool ValidatePaymentDetails(PaymentRequestDto paymentRequest)
        {
            if (!Regex.IsMatch(paymentRequest.CardNumber, @"^\d{16}$")) return false;
            if (!Regex.IsMatch(paymentRequest.ExpiryDate, @"^(0[1-9]|1[0-2])\/\d{2}$")) return false;
            if (!Regex.IsMatch(paymentRequest.CVV, @"^\d{3}$")) return false;

            var expiryParts = paymentRequest.ExpiryDate.Split('/');
            int expiryMonth = int.Parse(expiryParts[0]);
            int expiryYear = int.Parse(expiryParts[1]) + 2000;

            var expiryDate = new DateTime(expiryYear, expiryMonth, DateTime.DaysInMonth(expiryYear, expiryMonth));
            return expiryDate >= DateTime.Now;
        }

        private void PublishPaymentResponse(PaymentRequestDto request, bool isSuccess, string message)
        {
            var response = new PaymentResponseDto
            {
                UserId = request.UserId,
                CorrelationId = request.CorrelationId,
                IsSuccessful = isSuccess,
                Message = message
            };

            _messageBus.PublishMessage(_paymentResponseQueue, JsonConvert.SerializeObject(response));
        }
    }
}
