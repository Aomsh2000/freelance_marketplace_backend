// Controllers/ChatsController.cs
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("check")]
        public async Task<ActionResult<ChatCheckResponseDTO>> CheckChatExists(
            [FromQuery] string clientId, [FromQuery] string freelancerId)
        {
            return await _chatService.CheckChatExistsAsync(clientId, freelancerId);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ChatDTO>> CreateChat(CreateChatDto request)
        {
            try
            {
                return await _chatService.CreateChatAsync(request);
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
                return await _chatService.GetChatMessagesAsync(chatId, userId);
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
                return await _chatService.SendMessageAsync(chatId, request);
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
                return await _chatService.GetUserChatsAsync(userId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}