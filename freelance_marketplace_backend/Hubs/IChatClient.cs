// Create this file in your backend project: Hubs/IChatClient.cs
using freelance_marketplace_backend.Models.Dtos;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Hubs
{
    // This interface defines the methods that can be called from the server to the client
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDTO message);
        Task JoinChatSuccess(string chatId);
        Task LeaveChatSuccess(string chatId);
        Task JoinChatError(string error);
        Task LeaveChatError(string error);
        Task UserRegistered(string userId);
        Task ConnectionEstablished(string connectionId);
    }
}