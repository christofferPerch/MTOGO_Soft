namespace MTOGO.Services.PaymentAPI.Models.Dto
{
    public class CartResponseMessageDto
    {
        public string UserId { get; set; }
        public Guid CorrelationId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
    }
}
