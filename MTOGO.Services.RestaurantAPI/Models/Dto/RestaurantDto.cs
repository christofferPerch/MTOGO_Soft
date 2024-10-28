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
        public FoodCategory FoodCategory { get; set; }
        public int OperatingHoursId { get; set; }
        public OperatingHoursDto OperatingHours { get; set; }
        public int AddressId { get; set; }
        public AddressDto Address { get; set; }
        public List<MenuItemDto> MenuItems { get; set; }

    }

    public enum FoodCategory
    {
        Undefined = 0,    
        Chinese = 1,
        Pizza = 2,
        Burger = 3,
        Italian = 4,
        Indian = 5,
        Mexican = 6,
        Sushi = 7,
        Thai = 8,
        American = 9,
        Mediterranean = 10,
        Vegetarian = 11,
        Vegan = 12,
        Seafood = 13,
        French = 14,
        Dessert = 15,
        BBQ = 16,
        Steakhouse = 17,
        MiddleEastern = 18,
        FastFood = 19,
        Healthy = 20,
        Bakery = 21,
        Breakfast = 22,
        Coffee = 23
    }
}
