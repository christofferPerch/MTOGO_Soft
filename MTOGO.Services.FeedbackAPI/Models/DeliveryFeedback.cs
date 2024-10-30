namespace MTOGO.Services.FeedbackAPI.Models {
    public class DeliveryFeedback {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty!;
        public int DeliveryAgentId { get; set; }
        public int DeliveryExperienceRating { get; set; }
        public string? Comments { get; set; }
        public DateTime FeedbackTimestamp { get; set; }
    }
}
