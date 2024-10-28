namespace MTOGO.Services.OrderAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public int DeliveryAgentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public DateTime OrderPlacedTimestamp { get; set; }
        public int OrderStatusId { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
