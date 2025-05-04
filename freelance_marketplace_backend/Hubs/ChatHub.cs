using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace freelance_marketplace_backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        // Dictionary to track users in chat rooms - key: chatId, value: list of connectionIds
        private static readonly Dictionary<string, HashSet<string>> _chatRoomConnections =
            new Dictionary<string, HashSet<string>>();

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Join a specific chat group (room)
        public async Task JoinChat(string chatId)
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"User connection {connectionId} joining chat {chatId}");

            try
            {
                // Add to SignalR group
                await Groups.AddToGroupAsync(connectionId, chatId);

                // Track connection in our dictionary
                lock (_chatRoomConnections)
                {
                    if (!_chatRoomConnections.ContainsKey(chatId))
                    {
                        _chatRoomConnections[chatId] = new HashSet<string>();
                    }
                    _chatRoomConnections[chatId].Add(connectionId);
                }

                _logger.LogInformation($"User connection {connectionId} successfully joined chat {chatId}");

                // Confirm successful join to the client
                await Clients.Caller.SendAsync("JoinChatSuccess", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining chat {chatId} for connection {connectionId}");
                throw;
            }
        }

        // Leave a specific chat group
        public async Task LeaveChat(string chatId)
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"User connection {connectionId} leaving chat {chatId}");

            try
            {
                // Remove from SignalR group
                await Groups.RemoveFromGroupAsync(connectionId, chatId);

                // Remove from our tracking dictionary
                lock (_chatRoomConnections)
                {
                    if (_chatRoomConnections.ContainsKey(chatId))
                    {
                        _chatRoomConnections[chatId].Remove(connectionId);

                        // Clean up empty sets
                        if (_chatRoomConnections[chatId].Count == 0)
                        {
                            _chatRoomConnections.Remove(chatId);
                        }
                    }
                }

                _logger.LogInformation($"User connection {connectionId} successfully left chat {chatId}");

                // Confirm successful leave to the client
                await Clients.Caller.SendAsync("LeaveChatSuccess", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving chat {chatId} for connection {connectionId}");
                throw;
            }
        }

        // Get connection ID (useful for debugging)
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        // Debug method to list all connections in a chat room
        public List<string> GetConnectionsInChat(string chatId)
        {
            lock (_chatRoomConnections)
            {
                if (_chatRoomConnections.ContainsKey(chatId))
                {
                    return _chatRoomConnections[chatId].ToList();
                }
                return new List<string>();
            }
        }

        // Handle connection events
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"New SignalR connection established: {connectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;

            if (exception != null)
            {
                _logger.LogWarning(exception, $"SignalR connection {connectionId} disconnected with error");
            }
            else
            {
                _logger.LogInformation($"SignalR connection {connectionId} disconnected gracefully");
            }

            // Remove user from all chat rooms they were in
            List<string> roomsToCleanup = new List<string>();

            lock (_chatRoomConnections)
            {
                foreach (var kvp in _chatRoomConnections)
                {
                    string roomId = kvp.Key;
                    HashSet<string> connections = kvp.Value;

                    if (connections.Contains(connectionId))
                    {
                        connections.Remove(connectionId);
                        _logger.LogInformation($"Removed disconnected connection {connectionId} from chat {roomId}");

                        // Mark empty rooms for cleanup
                        if (connections.Count == 0)
                        {
                            roomsToCleanup.Add(roomId);
                        }
                    }
                }

                // Clean up empty rooms
                foreach (var roomId in roomsToCleanup)
                {
                    _chatRoomConnections.Remove(roomId);
                    _logger.LogInformation($"Removed empty chat room: {roomId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}