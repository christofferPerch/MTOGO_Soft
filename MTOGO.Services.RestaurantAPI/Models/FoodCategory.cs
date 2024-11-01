namespace MTOGO.Services.RestaurantAPI.Models
{
    public class FoodCategory
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public Category Category { get; set; }

        public Restaurant Restaurant { get; set; }
    }

    public enum Category
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
