using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _cacheExpirationTime = TimeSpan.FromMinutes(15);

        public ChatsController(IChatService chatService, IDistributedCache cache)
        {
            _chatService = chatService;
            _cache = cache;
        }

        [HttpGet("check")]
        public async Task<ActionResult<ChatCheckResponseDTO>> CheckChatExists(
            [FromQuery] string clientId, [FromQuery] string freelancerId)
        {
            // Verify user is either the client or freelancer
            var uid = User.FindFirst("user_id")?.Value;
            if (uid != clientId && uid != freelancerId)
            {
                return Unauthorized("You are not authorized to check this chat.");
            }

            return await _chatService.CheckChatExistsAsync(clientId, freelancerId);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ChatDTO>> CreateChat(CreateChatDto request)
        {
            try
            {
                // Verify user is either the client or freelancer
                var uid = User.FindFirst("user_id")?.Value;
                if (uid != request.ClientId && uid != request.FreelancerId)
                {
                    return Unauthorized("You are not authorized to create this chat.");
                }

                var createdChat = await _chatService.CreateChatAsync(request);

                // Invalidate user chats cache for both users
                await _cache.RemoveAsync($"user_chats_{request.ClientId}");
                await _cache.RemoveAsync($"user_chats_{request.FreelancerId}");

                return createdChat;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<List<MessageDTO>>> GetChatMessages(
            int chatId, [FromQuery] string userId)
        {
            try
            {
                // Verify user is the requested userId
                var uid = User.FindFirst("user_id")?.Value;
                if (uid != userId)
                {
                    return Unauthorized("You are not authorized to access these messages.");
                }

                // Try to get cached messages
                string cacheKey = $"chat_messages_{chatId}";
                byte[] cachedData = await _cache.GetAsync(cacheKey);

                if (cachedData != null)
                {
                    var cachedMessages = JsonSerializer.Deserialize<List<MessageDTO>>(cachedData);
                    return cachedMessages;
                }

                // If not in cache, get from service
                var messages = await _chatService.GetChatMessagesAsync(chatId, userId);

                // Cache the messages
                var serializedData = JsonSerializer.SerializeToUtf8Bytes(messages);
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(_cacheExpirationTime);

                await _cache.SetAsync(cacheKey, serializedData, cacheOptions);

                return messages;
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{chatId}/messages")]
        public async Task<ActionResult<MessageDTO>> SendMessage(
            int chatId, SendMessageDTO request)
        {
            try
            {
                // Verify user is the message sender
                var uid = User.FindFirst("user_id")?.Value;
                if (uid != request.SenderId)
                {
                    return Unauthorized("You are not authorized to send messages as this user.");
                }

                var message = await _chatService.SendMessageAsync(chatId, request);

                // Invalidate chat messages cache
                await _cache.RemoveAsync($"chat_messages_{chatId}");

                return message;
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ChatDTO>>> GetUserChats(string userId)
        {
            try
            {
                // Verify user is the requested userId
                var uid = User.FindFirst("user_id")?.Value;
                if (uid != userId)
                {
                    return Unauthorized("You are not authorized to access these chats.");
                }

                // Try to get cached chats
                string cacheKey = $"user_chats_{userId}";
                byte[] cachedData = await _cache.GetAsync(cacheKey);

                if (cachedData != null)
                {
                    var cachedChats = JsonSerializer.Deserialize<List<ChatDTO>>(cachedData);
                    return cachedChats;
                }

                // If not in cache, get from service
                var chats = await _chatService.GetUserChatsAsync(userId);

                // Cache the chats
                var serializedData = JsonSerializer.SerializeToUtf8Bytes(chats);
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(_cacheExpirationTime);

                await _cache.SetAsync(cacheKey, serializedData, cacheOptions);

                return chats;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}