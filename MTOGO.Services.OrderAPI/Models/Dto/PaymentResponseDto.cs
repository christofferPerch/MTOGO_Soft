namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class PaymentResponseDto
    {
        public string UserId { get; set; }
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }
}
