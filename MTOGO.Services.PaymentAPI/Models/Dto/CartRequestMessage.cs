namespace MTOGO.Services.PaymentAPI.Models.Dto
{
    public class CartRequestMessage
    {
        public string UserId { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
