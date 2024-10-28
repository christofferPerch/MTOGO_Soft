namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class AddRestaurantDto
    {
        public string RestaurantName { get; set; }
        public string LegalName { get; set; }
        public string RestaurantDescription { get; set; }
        public string VATNumber { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public FoodCategory FoodCategory { get; set; }
        public AddressDto Address { get; set; }
        public OperatingHoursDto OperatingHours { get; set; }
    }
}
