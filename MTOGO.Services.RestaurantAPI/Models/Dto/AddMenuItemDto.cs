namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class AddMenuItemDto
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public byte[] ?Image { get; set; }

    }
}
