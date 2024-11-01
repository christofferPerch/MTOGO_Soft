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
        public List<FoodCategoryDto> FoodCategories { get; set; } = new List<FoodCategoryDto>();
        public AddressDto Address { get; set; }
        public List<OperatingHoursDto> OperatingHours { get; set; } = new List<OperatingHoursDto>();
    }
}
