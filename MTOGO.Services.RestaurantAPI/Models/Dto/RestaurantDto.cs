namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class RestaurantDto
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public string LegalName { get; set; }
        public string RestaurantDescription { get; set; }
        public string VATNumber { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        public int AddressId { get; set; }

        public AddressDto Address { get; set; }
        public List<OperatingHoursDto> OperatingHours { get; set; } = new List<OperatingHoursDto>();
        public List<MenuItemDto> MenuItems { get; set; } = new List<MenuItemDto>();
        public List<FoodCategoryDto> FoodCategories { get; set; } = new List<FoodCategoryDto>();
    }
}
