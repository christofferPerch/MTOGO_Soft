namespace MTOGO.Services.FeedbackAPI.Models.Dto {
    public class RestaurantFeedbackDto {
        public string CustomerId { get; set; } = string.Empty!;
        public int FoodRating { get; set; }
        public string? Comments { get; set; }
    }
}
