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

        // Track connections and chat rooms
        private static readonly Dictionary<string, HashSet<string>> _chatRooms =
            new Dictionary<string, HashSet<string>>();
        private static readonly Dictionary<string, string> _userConnections =
            new Dictionary<string, string>();

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Join a specific chat group
        public async Task JoinChat(string chatId)
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"Connection {connectionId} joining chat {chatId}");

            try
            {
                // Add to SignalR group
                await Groups.AddToGroupAsync(connectionId, chatId);

                // Track in our dictionary
                lock (_chatRooms)
                {
                    if (!_chatRooms.ContainsKey(chatId))
                    {
                        _chatRooms[chatId] = new HashSet<string>();
                    }
                    _chatRooms[chatId].Add(connectionId);
                }

                _logger.LogInformation($"Connection {connectionId} successfully joined chat {chatId}");

                // Notify client of successful join
                await Clients.Caller.SendAsync("JoinChatSuccess", chatId);

                // Log current group members
                _logger.LogInformation($"Chat {chatId} now has {_chatRooms[chatId].Count} connections");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining chat {chatId}: {ex.Message}");
                throw;
            }
        }

        // Leave a specific chat group
        public async Task LeaveChat(string chatId)
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"Connection {connectionId} leaving chat {chatId}");

            try
            {
                // Remove from SignalR group
                await Groups.RemoveFromGroupAsync(connectionId, chatId);

                // Update tracking
                lock (_chatRooms)
                {
                    if (_chatRooms.ContainsKey(chatId) && _chatRooms[chatId].Contains(connectionId))
                    {
                        _chatRooms[chatId].Remove(connectionId);

                        // Clean up empty chat rooms
                        if (_chatRooms[chatId].Count == 0)
                        {
                            _chatRooms.Remove(chatId);
                            _logger.LogInformation($"Chat room {chatId} removed (no connections)");
                        }
                        else
                        {
                            _logger.LogInformation($"Chat {chatId} now has {_chatRooms[chatId].Count} connections");
                        }
                    }
                }

                // Notify client
                await Clients.Caller.SendAsync("LeaveChatSuccess", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving chat {chatId}: {ex.Message}");
                throw;
            }
        }

        // Get connection ID (useful for debugging)
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        // Associate user ID with connection ID
        public void RegisterUserId(string userId)
        {
            string connectionId = Context.ConnectionId;

            lock (_userConnections)
            {
                _userConnections[userId] = connectionId;
            }

            _logger.LogInformation($"User {userId} registered with connection {connectionId}");
        }

        // Test sending message to chat room
        public async Task SendTestMessage(string chatId, string content)
        {
            _logger.LogInformation($"Sending test message to chat {chatId}");

            await Clients.Group(chatId).SendAsync(
                "ReceiveMessage",
                new
                {
                    messageId = -1,
                    chatId = int.Parse(chatId),
                    senderId = "TEST",
                    content = $"TEST MESSAGE: {content}",
                    sentAt = DateTime.UtcNow
                });
        }

        // List connections in a chat room (for debugging)
        public List<string> GetConnectionsInRoom(string chatId)
        {
            lock (_chatRooms)
            {
                if (_chatRooms.ContainsKey(chatId))
                {
                    return _chatRooms[chatId].ToList();
                }
                return new List<string>();
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

            // Clean up chat room memberships
            List<string> roomsToUpdate = new List<string>();

            lock (_chatRooms)
            {
                foreach (var room in _chatRooms)
                {
                    if (room.Value.Contains(connectionId))
                    {
                        room.Value.Remove(connectionId);
                        roomsToUpdate.Add(room.Key);
                    }
                }

                // Clean up empty rooms
                foreach (var roomId in roomsToUpdate)
                {
                    if (_chatRooms[roomId].Count == 0)
                    {
                        _chatRooms.Remove(roomId);
                        _logger.LogInformation($"Chat room {roomId} removed (no connections)");
                    }
                    else
                    {
                        _logger.LogInformation($"Chat {roomId} now has {_chatRooms[roomId].Count} connections");
                    }
                }
            }

            // Clean up user connections
            lock (_userConnections)
            {
                var userToRemove = _userConnections.FirstOrDefault(x => x.Value == connectionId).Key;
                if (userToRemove != null)
                {
                    _userConnections.Remove(userToRemove);
                    _logger.LogInformation($"User {userToRemove} connection removed");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}