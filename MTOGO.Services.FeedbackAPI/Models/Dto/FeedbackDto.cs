﻿namespace MTOGO.Services.FeedbackAPI.Models.Dto {
    public class FeedbackDto {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int DeliveryAgentId { get; set; }
        public int FoodRating { get; set; } // Range 1-5
        public int DeliveryExperienceRating { get; set; } // Range 1-5
        public int DeliveryAgentRating { get; set; } // Range 1-5
        public string? Comments { get; set; }
        public DateTime FeedbackTimestamp { get; set; }
    }
}
