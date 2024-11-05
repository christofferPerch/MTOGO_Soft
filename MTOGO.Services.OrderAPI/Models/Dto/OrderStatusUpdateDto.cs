namespace MTOGO.Services.OrderAPI.Models.Dto {
    public class OrderStatusUpdateDto {
        public int OrderId { get; set; }
        public int StatusId { get; set; }  // Matches the enum value of OrderStatus
    }
}
