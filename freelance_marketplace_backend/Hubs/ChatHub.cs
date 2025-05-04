using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        // Thread-safe collections for tracking
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections =
            new ConcurrentDictionary<string, HashSet<string>>();

        private static readonly ConcurrentDictionary<string, HashSet<string>> _chatGroups =
            new ConcurrentDictionary<string, HashSet<string>>();

        private static readonly ConcurrentDictionary<string, string> _connectionToUser =
            new ConcurrentDictionary<string, string>();

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Register user ID with connection
        public async Task RegisterUser(string userId)
        {
            try
            {
                string connectionId = Context.ConnectionId;
                _logger.LogInformation($"Registering user {userId} with connection {connectionId}");

                // Add to connection-to-user mapping
                _connectionToUser.AddOrUpdate(connectionId, userId, (key, oldValue) => userId);

                // Add to user connections
                _userConnections.AddOrUpdate(
                    userId,
                    // If key doesn't exist, create new HashSet with this connection
                    new HashSet<string> { connectionId },
                    // If key exists, add this connection to the existing HashSet
                    (key, oldValue) =>
                    {
                        oldValue.Add(connectionId);
                        return oldValue;
                    }
                );

                _logger.LogInformation($"User {userId} registered with connection {connectionId}");
                _logger.LogInformation($"User {userId} now has {_userConnections[userId].Count} active connections");

                await Clients.Caller.SendAsync("UserRegistered", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user: {ex.Message}");
                throw;
            }
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
                _chatGroups.AddOrUpdate(
                    chatId,
                    // If key doesn't exist, create new HashSet with this connection
                    new HashSet<string> { connectionId },
                    // If key exists, add this connection to the existing HashSet
                    (key, oldValue) =>
                    {
                        oldValue.Add(connectionId);
                        return oldValue;
                    }
                );

                _logger.LogInformation($"Connection {connectionId} successfully joined chat {chatId}");

                if (_chatGroups.TryGetValue(chatId, out var connections))
                {
                    _logger.LogInformation($"Chat {chatId} now has {connections.Count} connections");
                }

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
                if (_chatGroups.TryGetValue(chatId, out var connections))
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _chatGroups.TryRemove(chatId, out _);
                        _logger.LogInformation($"Removed empty chat group {chatId}");
                    }
                    else
                    {
                        _logger.LogInformation($"Chat {chatId} now has {connections.Count} connections");
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

        // Connection handling
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation($"New connection established: {connectionId}");

            await Clients.Caller.SendAsync("ConnectionEstablished", connectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;

            // Get the user ID associated with this connection
            string userId = null;
            _connectionToUser.TryRemove(connectionId, out userId);

            // Remove from all chat groups
            foreach (var chatGroup in _chatGroups)
            {
                if (chatGroup.Value.Contains(connectionId))
                {
                    chatGroup.Value.Remove(connectionId);
                    _logger.LogInformation($"Removed connection {connectionId} from chat {chatGroup.Key}");

                    if (chatGroup.Value.Count == 0)
                    {
                        _chatGroups.TryRemove(chatGroup.Key, out _);
                        _logger.LogInformation($"Removed empty chat group {chatGroup.Key}");
                    }
                }
            }

            // Remove from user connections
            if (userId != null && _userConnections.TryGetValue(userId, out var userConns))
            {
                userConns.Remove(connectionId);
                _logger.LogInformation($"Removed connection {connectionId} from user {userId}");

                if (userConns.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                    _logger.LogInformation($"Removed user {userId} with no active connections");
                }
                else
                {
                    _logger.LogInformation($"User {userId} still has {userConns.Count} active connections");
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

        // Get all active connections for a user
        public IEnumerable<string> GetUserConnections(string userId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                return connections;
            }

            return new List<string>();
        }

        // Get all connections in a chat room
        public IEnumerable<string> GetChatRoomConnections(string chatId)
        {
            if (_chatGroups.TryGetValue(chatId, out var connections))
            {
                return connections;
            }

            return new List<string>();
        }
    }
}