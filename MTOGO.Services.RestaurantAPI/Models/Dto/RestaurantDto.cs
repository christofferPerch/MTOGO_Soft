namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class RestaurantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddressId { get; set; }
        public AddressDto Address { get; set; }
        public string ContactInformation { get; set; }
        public int OperatingHoursId { get; set; }
        public OperatingHoursDto OperatingHours { get; set; }
        public List<MenuItemDto> MenuItems { get; set; }
        public List<FeeStructureDto> FeeStructures { get; set; }
    }
}
