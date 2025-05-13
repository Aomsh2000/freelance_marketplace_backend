using freelance_marketplace_backend.Models.Dtos;

namespace freelance_marketplace_backend.Interfaces
{
    public interface IChatService
    {
        Task<ChatCheckResponseDTO> CheckChatExistsAsync(string clientId, string freelancerId);
        Task<ChatDTO> CreateChatAsync(CreateChatDto request);
        Task<List<ChatDTO>> GetUserChatsAsync(string userId);
        Task<MessageDTO> SendMessageAsync(int chatId, SendMessageDTO request);
        Task<List<MessageDTO>> GetChatMessagesAsync(int chatId, string userId);
    }
}