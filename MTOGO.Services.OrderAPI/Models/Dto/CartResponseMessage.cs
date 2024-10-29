namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class CartResponseMessage
    {
        public string UserId { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
