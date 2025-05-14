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
using Microsoft.Extensions.Logging;

namespace freelance_marketplace_backend.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatRepository _chatRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            ChatRepository chatRepository,
            IHubContext<ChatHub> hubContext,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _hubContext = hubContext;
            _logger = logger;
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
            var otherUserId = request.ClientId == chat.ClientId ? chat.FreelancerId : chat.ClientId;

            // Load the full chat with user details
            var fullChat = await _chatRepository.GetChatByIdAsync(chat.ChatId);
            var otherUser = otherUserId == fullChat.ClientId ? fullChat.Client : fullChat.Freelancer;

            return new ChatDTO
            {
                ChatId = chat.ChatId,
                ClientId = chat.ClientId,
                FreelancerId = chat.FreelancerId,
                StartedAt = chat.StartedAt,
                OtherUserName = otherUser?.Name ?? "Unknown User",
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
                    OtherUserImageUrl = otherUser?.ImageUrl ?? "https://www.svgrepo.com/show/384670/account-avatar-profile-user.svg", // Add this line
                    LastMessage = lastMessage?.Content,
                    LastMessageTime = lastMessage?.SentAt,
                    LastMessageSenderId = lastMessage?.SenderId,
                    IsLastMessageFromMe = lastMessage != null && lastMessage.SenderId == userId
                });
            }

            return chatDtos;
        }

        public async Task<MessageDTO> SendMessageAsync(int chatId, SendMessageDTO request)
        {
            try
            {
                // Validate user is part of the chat
                var isUserInChat = await _chatRepository.IsUserInChatAsync(chatId, request.SenderId);
                if (!isUserInChat)
                {
                    throw new UnauthorizedAccessException("User is not a participant in this chat");
                }

                // Create message
                var message = await _chatRepository.CreateMessageAsync(chatId, request.SenderId, request.Content);

                // Get chat info to identify other users
                var chat = await _chatRepository.GetChatByIdAsync(chatId);

                // Create DTO - WITHOUT setting IsFromMe (let the client handle this)
                var messageDto = new MessageDTO
                {
                    MessageId = message.MessageId,
                    ChatId = message.ChatId,
                    SenderId = message.SenderId,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsFromMe = false  // Let the client set this based on their own identity
                };

                // Log the outgoing message
                _logger.LogInformation($"Broadcasting message ID {message.MessageId} to chat group {chatId}");

                // First attempt - broadcast to the group
                try
                {
                    await _hubContext.Clients.Group(chatId.ToString())
                        .SendAsync("ReceiveMessage", messageDto);

                    _logger.LogInformation($"Successfully broadcast message to chat group {chatId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error broadcasting message to chat group {chatId}: {ex.Message}");
                }

                // Make multiple attempts to ensure delivery
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Wait briefly before re-sending to handle race conditions
                        await Task.Delay(500);

                        // Try again with group
                        await _hubContext.Clients.Group(chatId.ToString())
                            .SendAsync("ReceiveMessage", messageDto);

                        _logger.LogInformation($"Re-broadcast message to chat group {chatId} after 500ms");

                        // Wait a bit longer for any slow connections
                        await Task.Delay(2000);

                        // Try one more time to ensure delivery
                        await _hubContext.Clients.Group(chatId.ToString())
                            .SendAsync("ReceiveMessage", messageDto);

                        _logger.LogInformation($"Final broadcast message to chat group {chatId} after 2.5s");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error in delayed re-broadcasting: {ex.Message}");
                    }
                });

                // The sender gets a different version with IsFromMe = true
                messageDto.IsFromMe = true;
                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to chat {chatId}: {ex.Message}");
                throw;
            }
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