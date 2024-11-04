namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class CreateOrderDto
    {
        public string UserId { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
