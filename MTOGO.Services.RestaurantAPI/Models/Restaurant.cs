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
        public int AddressId { get; set; }
        public Address Address { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public int OperatingHoursId { get; set; }
        public OperatingHours OperatingHours { get; set; }
        public ICollection<MenuItem> MenuItems { get; set; }
        public FoodCategory FoodCategory { get; set; }

    }
}
