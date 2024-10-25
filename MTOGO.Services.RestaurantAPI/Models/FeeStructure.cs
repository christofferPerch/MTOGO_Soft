namespace MTOGO.Services.RestaurantAPI.Models
{
    public class FeeStructure
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public decimal MaximumOrderAmount { get; set; }
        public decimal FeePercentage { get; set; }
    }
}
