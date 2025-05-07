using freelance_marketplace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Data.Repositories
{
    public class ChatRepository
    {
        private readonly FreelancingPlatformContext _context;

        public ChatRepository(FreelancingPlatformContext context)
        {
            _context = context;
        }

        public async Task<bool> ChatExistsAsync(string userId1, string userId2)
        {
            return await _context.Chats
                .AnyAsync(c =>
                    ((c.ClientId == userId1 && c.FreelancerId == userId2) ||
                     (c.ClientId == userId2 && c.FreelancerId == userId1))
                    && c.IsDeleted != true);
        }


        public async Task<Chat> GetChatByClientAndFreelancerAsync(string userId1, string userId2)
        {
            return await _context.Chats
                .FirstOrDefaultAsync(c =>
                    ((c.ClientId == userId1 && c.FreelancerId == userId2) ||
                     (c.ClientId == userId2 && c.FreelancerId == userId1))
                    && c.IsDeleted != true);
        }

        public async Task<Chat> CreateChatAsync(string clientId, string freelancerId)
        {
            var chat = new Chat
            {
                ClientId = clientId,
                FreelancerId = freelancerId,
                StartedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            return await _context.Chats
                .Where(c => (c.ClientId == userId || c.FreelancerId == userId) && c.IsDeleted != true)
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .ToListAsync();
        }

        public async Task<Chat> GetChatByIdAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .FirstOrDefaultAsync(c => c.ChatId == chatId && c.IsDeleted != true);
        }

        public async Task<Message> CreateMessageAsync(int chatId, string senderId, string content)
        {
            var message = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetChatMessagesAsync(int chatId)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId && m.IsDeleted != true)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<bool> IsUserInChatAsync(int chatId, string userId)
        {
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.ChatId == chatId && c.IsDeleted != true);

            return chat != null && (chat.ClientId == userId || chat.FreelancerId == userId);
        }
    }
}