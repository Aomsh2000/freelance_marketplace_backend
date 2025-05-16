using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using freelance_marketplace_backend.Data.Repositories;

namespace freelance_marketplace_backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatRepository _chatRepository;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ChatRepository chatRepository, ILogger<ChatHub> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        // Connection management
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Join a specific chat
        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            _logger.LogInformation($"User joined chat {chatId}");
        }

        // Leave a specific chat
        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
            _logger.LogInformation($"User left chat {chatId}");
        }

        // Check if chat exists
        public async Task<ChatCheckResponseDTO> CheckChatExists(string clientId, string freelancerId)
        {
            bool exists = await _chatRepository.ChatExistsAsync(clientId, freelancerId);

            if (!exists)
            {
                return new ChatCheckResponseDTO { Exists = false, ChatId = 0 };
            }

            var chat = await _chatRepository.GetChatByClientAndFreelancerAsync(clientId, freelancerId);
            return new ChatCheckResponseDTO
            {
                Exists = true,
                ChatId = chat.ChatId
            };
        }

        // Create a new chat
        public async Task<ChatDTO> CreateChat(string clientId, string freelancerId)
        {
            // Check if chat already exists
            var existingChat = await _chatRepository.GetChatByClientAndFreelancerAsync(
                clientId, freelancerId);

            Chat chat;
            if (existingChat != null)
            {
                chat = existingChat;
            }
            else
            {
                chat = await _chatRepository.CreateChatAsync(clientId, freelancerId);
            }

            // Load the full chat with user details
            var fullChat = await _chatRepository.GetChatByIdAsync(chat.ChatId);
            var otherUserId = clientId == chat.ClientId ? chat.FreelancerId : chat.ClientId;
            var otherUser = otherUserId == fullChat.ClientId ? fullChat.Client : fullChat.Freelancer;

            return new ChatDTO
            {
                ChatId = chat.ChatId,
                ClientId = chat.ClientId,
                FreelancerId = chat.FreelancerId,
                StartedAt = chat.StartedAt,
                OtherUserName = otherUser?.Name ?? "Unknown User",
                OtherUserImageUrl = otherUser?.ImageUrl ?? "https://www.svgrepo.com/show/384670/account-avatar-profile-user.svg",
                LastMessage = null,
                LastMessageTime = null,
                IsLastMessageFromMe = false
            };
        }

        // Get user's chats
        public async Task<List<ChatDTO>> GetUserChats(string userId)
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
                    OtherUserImageUrl = otherUser?.ImageUrl ?? "https://www.svgrepo.com/show/384670/account-avatar-profile-user.svg",
                    LastMessage = lastMessage?.Content,
                    LastMessageTime = lastMessage?.SentAt,
                    LastMessageSenderId = lastMessage?.SenderId,
                    IsLastMessageFromMe = lastMessage != null && lastMessage.SenderId == userId
                });
            }

            return chatDtos;
        }

        // Send a message
        public async Task<MessageDTO> SendMessage(int chatId, string senderId, string content)
        {
            try
            {
                // Validate user is part of the chat
                var isUserInChat = await _chatRepository.IsUserInChatAsync(chatId, senderId);
                if (!isUserInChat)
                {
                    throw new HubException("User is not a participant in this chat");
                }

                // Create message
                var message = await _chatRepository.CreateMessageAsync(chatId, senderId, content);

                // Create DTO
                var messageDto = new MessageDTO
                {
                    MessageId = message.MessageId,
                    ChatId = message.ChatId,
                    SenderId = message.SenderId,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsFromMe = false  // Let the client set this
                };

                // Broadcast to all clients in the group
                await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", messageDto);

                // The sender gets a different version with IsFromMe = true
                messageDto.IsFromMe = true;
                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to chat {chatId}");
                throw;
            }
        }

        // Get chat messages
        public async Task<List<MessageDTO>> GetChatMessages(int chatId, string userId)
        {
            // Validate user is part of the chat
            var isUserInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
            if (!isUserInChat)
            {
                throw new HubException("User is not a participant in this chat");
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