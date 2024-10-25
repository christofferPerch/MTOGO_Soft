namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class FeeStructureDto
    {
        public decimal MinimumOrderAmount { get; set; }
        public decimal MaximumOrderAmount { get; set; }
        public decimal FeePercentage { get; set; }
    }
}
