using MTOGO.MessageBus;
using MTOGO.Services.PaymentAPI.Models.Dto;
using Newtonsoft.Json;

public class PaymentService
{
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IMessageBus messageBus, IConfiguration configuration, ILogger<PaymentService> logger)
    {
        _messageBus = messageBus;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> ProcessPayment(PaymentRequestDto paymentRequest)
    {
        var cartDetails = await GetCartDetails(paymentRequest.UserId, paymentRequest.CorrelationId);
        if (cartDetails == null)
        {
            return new PaymentResponseDto
            {
                UserId = paymentRequest.UserId,
                CorrelationId = paymentRequest.CorrelationId,
                IsSuccessful = false,
                Message = "Failed to retrieve shopping cart details."
            };
        }

        paymentRequest.Items = cartDetails.Items;
        paymentRequest.TotalAmount = cartDetails.Items.Sum(item => item.Price * item.Quantity);

        bool isPaymentValid = ValidatePaymentDetails(paymentRequest);

        var paymentResponse = new PaymentResponseDto
        {
            UserId = paymentRequest.UserId,
            CorrelationId = paymentRequest.CorrelationId,
            IsSuccessful = isPaymentValid,
            Message = isPaymentValid ? "Payment processed successfully." : "Payment failed."
        };

        if (isPaymentValid)
        {
            //await PublishOrderCreation(paymentRequest);
            //await PublishCartRemoval(paymentRequest.UserId);
            await PublishPaymentSuccess(paymentRequest);

        }

        return paymentResponse;
    }

    private async Task<CartResponseMessageDto?> GetCartDetails(string userId, Guid correlationId)
    {
        var cartRequest = new CartRequestMessage
        {
            UserId = userId,
            CorrelationId = correlationId
        };

        string cartRequestQueue = "CartRequestQueue"; 
        await _messageBus.PublishMessage(cartRequestQueue, JsonConvert.SerializeObject(cartRequest));

        var tcs = new TaskCompletionSource<CartResponseMessageDto>();

        _messageBus.SubscribeMessage<CartResponseMessageDto>("CartResponseQueue", message =>
        {
            if (message.CorrelationId == correlationId)
            {
                tcs.SetResult(message);
            }
        });

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30)));
        if (completedTask == tcs.Task)
        {
            return tcs.Task.Result;
        }
        else
        {
            _logger.LogWarning("Timeout waiting for cart response.");
            return null;
        }
    }

    private bool ValidatePaymentDetails(PaymentRequestDto paymentRequest)
    {
        return !string.IsNullOrEmpty(paymentRequest.CardNumber) &&
               !string.IsNullOrEmpty(paymentRequest.ExpiryDate) &&
               !string.IsNullOrEmpty(paymentRequest.CVV);
    }

    private Task PublishPaymentSuccess(PaymentRequestDto paymentRequest)
    {
        string paymentSuccessQueue = _configuration.GetValue<string>("TopicAndQueueNames:PaymentSuccessQueue");
        return _messageBus.PublishMessage(paymentSuccessQueue, JsonConvert.SerializeObject(paymentRequest));
    }


    private Task PublishOrderCreation(PaymentRequestDto paymentRequest)
    {
        string orderQueue = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedQueue");
        return _messageBus.PublishMessage(orderQueue, JsonConvert.SerializeObject(paymentRequest));
    }

    private Task PublishCartRemoval(string userId)
    {
        string cartRemovalQueue = _configuration.GetValue<string>("TopicAndQueueNames:CartRemovedQueue");
        var cartRemovedMessage = new { UserId = userId };
        return _messageBus.PublishMessage(cartRemovalQueue, JsonConvert.SerializeObject(cartRemovedMessage));
    }
}
