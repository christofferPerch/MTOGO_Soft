namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public byte[] ?Image { get; set; }
        public int RestaurantId { get; set; }
        public RestaurantDto? Restaurant { get; set; }
    }
}
