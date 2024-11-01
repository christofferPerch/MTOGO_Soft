namespace MTOGO.Services.RestaurantAPI.Models
{
    public class OperatingHours
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public DayEnum Day { get; set; }
        public TimeSpan OpeningHours { get; set; }
        public TimeSpan ClosingHours { get; set; }

        public Restaurant Restaurant { get; set; }
    }

    public enum DayEnum
    {
        Undefined = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }
}
