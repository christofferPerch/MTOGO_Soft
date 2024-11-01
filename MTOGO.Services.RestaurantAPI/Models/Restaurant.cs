using MTOGO.Services.RestaurantAPI.Models.Dto;

namespace MTOGO.Services.RestaurantAPI.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public string LegalName { get; set; }
        public string VATNumber { get; set; }
        public string RestaurantDescription { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public int AddressId { get; set; }
        public Address Address { get; set; }

        public ICollection<OperatingHours> OperatingHours { get; set; } = new List<OperatingHours>();
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<FoodCategory> FoodCategories { get; set; } = new List<FoodCategory>();
    }
}
