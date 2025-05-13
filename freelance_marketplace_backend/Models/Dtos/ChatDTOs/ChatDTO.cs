namespace freelance_marketplace_backend.Models.Dtos
{
    public class ChatDTO
    {
        public int ChatId { get; set; }
        public string ClientId { get; set; }
        public string FreelancerId { get; set; }
        public DateTime? StartedAt { get; set; }
        public string OtherUserName { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public string LastMessageSenderId { get; set; }  
        public bool IsLastMessageFromMe { get; set; }    
    }
}
