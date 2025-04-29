using freelance_marketplace_backend.Models.Entities;

namespace freelance_marketplace_backend.Interfaces
{
    public interface IChatRepository
    {
        Task<bool> ChatExistsAsync(string clientId, string freelancerId);
        Task<Chat> GetChatByClientAndFreelancerAsync(string clientId, string freelancerId);
        Task<Chat> CreateChatAsync(string clientId, string freelancerId);
        Task<List<Chat>> GetUserChatsAsync(string userId);
        Task<Chat> GetChatByIdAsync(int chatId);
        Task<Message> CreateMessageAsync(int chatId, string senderId, string content);
        Task<List<Message>> GetChatMessagesAsync(int chatId);
        Task<bool> IsUserInChatAsync(int chatId, string userId);
    }
}
