namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class OperatingHoursDto
    {
        public DayEnum Day { get; set; }
        public TimeSpan OpeningHours { get; set; }
        public TimeSpan ClosingHours { get; set; }
    }
}
