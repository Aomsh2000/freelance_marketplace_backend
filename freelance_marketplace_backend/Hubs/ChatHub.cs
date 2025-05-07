using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Hubs
{
    public class ChatHub : Hub
    {
        // Join a specific chat group (room)
        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        // Leave a specific chat group
        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }
    }
}