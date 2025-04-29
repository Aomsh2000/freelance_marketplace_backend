namespace freelance_marketplace_backend.Models.Dtos
{
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsFromMe { get; set; }
    }
}
