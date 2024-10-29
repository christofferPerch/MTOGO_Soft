namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class CartRequestMessage
    {
        public string UserId { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
