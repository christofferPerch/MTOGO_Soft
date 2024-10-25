namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class UpdateRestaurantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactInformation { get; set; }
        public AddressDto Address { get; set; }
        public OperatingHoursDto OperatingHours { get; set; }
        public FeeStructureDto FeeStructure { get; set; }
    }
}
