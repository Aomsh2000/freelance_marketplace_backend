using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        // Dictionary to track users in chats
        private static readonly Dictionary<string, HashSet<string>> _chatRoomConnections =
            new Dictionary<string, HashSet<string>>();

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Join a specific chat group (room)
        public async Task JoinChat(string chatId)
        {
            try
            {
                _logger.LogInformation($"User connection {Context.ConnectionId} joining chat {chatId}");

                // Add to SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);

                // Track connection in our dictionary
                lock (_chatRoomConnections)
                {
                    if (!_chatRoomConnections.ContainsKey(chatId))
                    {
                        _chatRoomConnections[chatId] = new HashSet<string>();
                    }
                    _chatRoomConnections[chatId].Add(Context.ConnectionId);
                }

                // Notify caller of successful join
                await Clients.Caller.SendAsync("JoinChatSuccess", chatId);

                _logger.LogInformation($"User connection {Context.ConnectionId} successfully joined chat {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining chat {chatId}");
                // Notify caller of error
                await Clients.Caller.SendAsync("JoinChatError", chatId, ex.Message);
                throw;
            }
        }

        // Leave a specific chat group
        public async Task LeaveChat(string chatId)
        {
            try
            {
                _logger.LogInformation($"User connection {Context.ConnectionId} leaving chat {chatId}");

                // Remove from SignalR group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);

                // Remove from our tracking dictionary
                lock (_chatRoomConnections)
                {
                    if (_chatRoomConnections.ContainsKey(chatId))
                    {
                        _chatRoomConnections[chatId].Remove(Context.ConnectionId);

                        // Clean up empty sets
                        if (_chatRoomConnections[chatId].Count == 0)
                        {
                            _chatRoomConnections.Remove(chatId);
                        }
                    }
                }

                // Notify caller of successful leave
                await Clients.Caller.SendAsync("LeaveChatSuccess", chatId);

                _logger.LogInformation($"User connection {Context.ConnectionId} successfully left chat {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving chat {chatId}");
                // Notify caller of error
                await Clients.Caller.SendAsync("LeaveChatError", chatId, ex.Message);
                throw;
            }
        }

        // Handle connection disconnections
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"User connection {Context.ConnectionId} disconnected");

            // Remove user from all chat rooms they were in
            List<string> roomsToCleanup = new List<string>();

            lock (_chatRoomConnections)
            {
                foreach (var room in _chatRoomConnections)
                {
                    if (room.Value.Contains(Context.ConnectionId))
                    {
                        room.Value.Remove(Context.ConnectionId);

                        // Mark empty rooms for cleanup
                        if (room.Value.Count == 0)
                        {
                            roomsToCleanup.Add(room.Key);
                        }
                    }
                }

                // Clean up empty rooms
                foreach (var roomId in roomsToCleanup)
                {
                    _chatRoomConnections.Remove(roomId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}