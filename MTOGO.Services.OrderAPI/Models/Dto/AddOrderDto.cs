namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class AddOrderDto
    {
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
