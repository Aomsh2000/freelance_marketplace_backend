using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Join a specific chat group (room)
        public async Task JoinChat(string chatId)
        {
            try
            {
                string connectionId = Context.ConnectionId;
                _logger.LogInformation($"Connection {connectionId} joining chat {chatId}");

                await Groups.AddToGroupAsync(connectionId, chatId);

                _logger.LogInformation($"Connection {connectionId} successfully joined chat {chatId}");

                // Confirm join successful
                await Clients.Caller.SendAsync("JoinChatSuccess", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining chat: {ex.Message}");

                // Notify client of error
                await Clients.Caller.SendAsync("JoinChatError", ex.Message);
                throw;
            }
        }

        // Leave a specific chat group
        public async Task LeaveChat(string chatId)
        {
            try
            {
                string connectionId = Context.ConnectionId;
                _logger.LogInformation($"Connection {connectionId} leaving chat {chatId}");

                await Groups.RemoveFromGroupAsync(connectionId, chatId);

                _logger.LogInformation($"Connection {connectionId} successfully left chat {chatId}");

                // Confirm leave successful
                await Clients.Caller.SendAsync("LeaveChatSuccess", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving chat: {ex.Message}");

                // Notify client of error
                await Clients.Caller.SendAsync("LeaveChatError", ex.Message);
                throw;
            }
        }

        // Connection handling
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"New connection established: {connectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;

            if (exception != null)
            {
                _logger.LogWarning($"Connection {connectionId} disconnected with error: {exception.Message}");
            }
            else
            {
                _logger.LogInformation($"Connection {connectionId} disconnected gracefully");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Get connection ID (useful for debugging)
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        // Test ping method to verify connection
        public string Ping()
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"Ping received from connection {connectionId}");
            return "Pong";
        }
    }
}