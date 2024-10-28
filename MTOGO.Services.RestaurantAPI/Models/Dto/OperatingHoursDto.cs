namespace MTOGO.Services.RestaurantAPI.Models.Dto
{
    public class OperatingHoursDto
    {
        public TimeSpan MondayOpening { get; set; }
        public TimeSpan MondayClosing { get; set; }
        public TimeSpan TuesdayOpening { get; set; }
        public TimeSpan TuesdayClosing { get; set; }
        public TimeSpan WednesdayOpening { get; set; }
        public TimeSpan WednesdayClosing { get; set; }
        public TimeSpan ThursdayOpening { get; set; }
        public TimeSpan ThursdayClosing { get; set; }
        public TimeSpan FridayOpening { get; set; }
        public TimeSpan FridayClosing { get; set; }
        public TimeSpan? SaturdayOpening { get; set; } 
        public TimeSpan? SaturdayClosing { get; set; }
        public TimeSpan? SundayOpening { get; set; } 
        public TimeSpan? SundayClosing { get; set; }
    }
}
