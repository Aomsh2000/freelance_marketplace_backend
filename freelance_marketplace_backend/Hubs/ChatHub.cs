using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private static readonly Dictionary<string, HashSet<string>> _userConnections = new Dictionary<string, HashSet<string>>();
        private static readonly Dictionary<string, HashSet<string>> _chatGroups = new Dictionary<string, HashSet<string>>();

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

                // Add to group
                await Groups.AddToGroupAsync(connectionId, chatId);

                // Track chat membership
                if (!_chatGroups.ContainsKey(chatId))
                {
                    _chatGroups[chatId] = new HashSet<string>();
                }
                _chatGroups[chatId].Add(connectionId);

                _logger.LogInformation($"Connection {connectionId} successfully joined chat {chatId}");
                _logger.LogInformation($"Chat {chatId} now has {_chatGroups[chatId].Count} connections");

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

                // Update tracking
                if (_chatGroups.ContainsKey(chatId))
                {
                    _chatGroups[chatId].Remove(connectionId);
                    if (_chatGroups[chatId].Count == 0)
                    {
                        _chatGroups.Remove(chatId);
                    }
                }

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

        // Register user ID with connection
        public async Task RegisterUser(string userId)
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"Registering user {userId} with connection {connectionId}");

            // Add to user connections
            if (!_userConnections.ContainsKey(userId))
            {
                _userConnections[userId] = new HashSet<string>();
            }
            _userConnections[userId].Add(connectionId);

            await Clients.Caller.SendAsync("UserRegistered", userId);
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

            // Remove from all chat groups
            foreach (var chatGroup in _chatGroups)
            {
                if (chatGroup.Value.Contains(connectionId))
                {
                    chatGroup.Value.Remove(connectionId);
                    if (chatGroup.Value.Count == 0)
                    {
                        _chatGroups.Remove(chatGroup.Key);
                    }
                }
            }

            // Remove from user connections
            foreach (var userConnection in _userConnections)
            {
                if (userConnection.Value.Contains(connectionId))
                {
                    userConnection.Value.Remove(connectionId);
                    if (userConnection.Value.Count == 0)
                    {
                        _userConnections.Remove(userConnection.Key);
                    }
                }
            }

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