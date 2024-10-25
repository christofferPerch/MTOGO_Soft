namespace MTOGO.Services.RestaurantAPI.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddressId { get; set; }
        public Address Address { get; set; }
        public string ContactInformation { get; set; }
        public int OperatingHoursId { get; set; }
        public OperatingHours OperatingHours { get; set; }
        public ICollection<MenuItem> MenuItems { get; set; }
        public ICollection<FeeStructure> FeeStructures { get; set; }
    }
}
