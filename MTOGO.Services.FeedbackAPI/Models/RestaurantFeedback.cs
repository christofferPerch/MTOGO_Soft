namespace MTOGO.Services.FeedbackAPI.Models {
    public class RestaurantFeedback {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty!;
        public int FoodRating { get; set; }
        public string? Comments { get; set; }
        public DateTime FeedbackTimestamp { get; set; }
    }
}
