// Services/ChatService.cs
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Hubs;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatRepository _chatRepository;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(
            ChatRepository chatRepository,
            IHubContext<ChatHub> hubContext)
        {
            _chatRepository = chatRepository;
            _hubContext = hubContext;
        }

        public async Task<ChatCheckResponseDTO> CheckChatExistsAsync(string userId1, string userId2)
        {
            var chatExists = await _chatRepository.ChatExistsAsync(userId1, userId2);

            if (!chatExists)
            {
                return new ChatCheckResponseDTO { Exists = false, ChatId = 0 };
            }

            var chat = await _chatRepository.GetChatByClientAndFreelancerAsync(userId1, userId2);
            return new ChatCheckResponseDTO
            {
                Exists = true,
                ChatId = chat.ChatId
            };
        }

        // In ChatService.cs, update the CreateChatAsync method to include user information

        public async Task<ChatDTO> CreateChatAsync(CreateChatDto request)
        {
            // Check if chat already exists
            var existingChat = await _chatRepository.GetChatByClientAndFreelancerAsync(
                request.ClientId, request.FreelancerId);

            Chat chat;
            if (existingChat != null)
            {
                // Use existing chat
                chat = existingChat;
            }
            else
            {
                // Create new chat
                chat = await _chatRepository.CreateChatAsync(request.ClientId, request.FreelancerId);
            }

            // Get the other user information
            // First determine which user is the "other" user relative to the requester
            var otherUserId = request.ClientId == chat.ClientId ? chat.FreelancerId : chat.ClientId;

            // Load the full chat with user details
            var fullChat = await _chatRepository.GetChatByIdAsync(chat.ChatId);
            var otherUser = otherUserId == fullChat.ClientId ? fullChat.Client : fullChat.Freelancer;

            // Return enhanced DTO with the other user's name
            return new ChatDTO
            {
                ChatId = chat.ChatId,
                ClientId = chat.ClientId,
                FreelancerId = chat.FreelancerId,
                StartedAt = chat.StartedAt,
                OtherUserName = otherUser?.Name ?? "Unknown User", // Include the name
                                                                   // Include other fields as needed
                LastMessage = null,
                LastMessageTime = null,
                IsLastMessageFromMe = false
            };
        }

        public async Task<List<ChatDTO>> GetUserChatsAsync(string userId)
        {
            var chats = await _chatRepository.GetUserChatsAsync(userId);
            var chatDtos = new List<ChatDTO>();

            foreach (var chat in chats)
            {
                var otherUser = chat.ClientId == userId ? chat.Freelancer : chat.Client;
                var lastMessage = chat.Messages.FirstOrDefault();

                chatDtos.Add(new ChatDTO
                {
                    ChatId = chat.ChatId,
                    ClientId = chat.ClientId,
                    FreelancerId = chat.FreelancerId,
                    StartedAt = chat.StartedAt,
                    OtherUserName = otherUser?.Name,
                    LastMessage = lastMessage?.Content,
                    LastMessageTime = lastMessage?.SentAt,
                    IsLastMessageFromMe = lastMessage != null && lastMessage.SenderId == userId
                });
            }

            return chatDtos;
        }

        public async Task<MessageDTO> SendMessageAsync(int chatId, SendMessageDTO request)
        {
            // Validate user is part of the chat
            var isUserInChat = await _chatRepository.IsUserInChatAsync(chatId, request.SenderId);
            if (!isUserInChat)
            {
                throw new UnauthorizedAccessException("User is not a participant in this chat");
            }

            // Create message
            var message = await _chatRepository.CreateMessageAsync(chatId, request.SenderId, request.Content);

            // Create DTO
            var messageDto = new MessageDTO
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsFromMe = true  // Always true for messages you just sent
            };

            // Broadcast to all clients in the chat group using SignalR
            await _hubContext.Clients.Group(chatId.ToString())
                .SendAsync("ReceiveMessage", messageDto);

            return messageDto;
        }

        public async Task<List<MessageDTO>> GetChatMessagesAsync(int chatId, string userId)
        {
            // Validate user is part of the chat
            var isUserInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
            if (!isUserInChat)
            {
                throw new UnauthorizedAccessException("User is not a participant in this chat");
            }

            var messages = await _chatRepository.GetChatMessagesAsync(chatId);

            var messageDtos = messages.Select(m => new MessageDTO
            {
                MessageId = m.MessageId,
                ChatId = m.ChatId,
                SenderId = m.SenderId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsFromMe = m.SenderId == userId  
            }).ToList();

            return messageDtos;
        }
    }
}