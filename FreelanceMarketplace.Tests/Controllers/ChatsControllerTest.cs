using freelance_marketplace_backend.Controllers;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace freelance_marketplace_backend.Tests.Controllers
{
    public class ChatsControllerTests
    {
        private readonly Mock<IChatService> _chatServiceMock;
        private readonly ChatsController _controller;

        public ChatsControllerTests()
        {
            _chatServiceMock = new Mock<IChatService>();
            _controller = new ChatsController(_chatServiceMock.Object);
        }

        [Fact]
        public async Task CheckChatExists_ReturnsExpectedResult()
        {
            var expected = new ChatCheckResponseDTO { Exists = true, ChatId = 5 };
            _chatServiceMock.Setup(s => s.CheckChatExistsAsync("client1", "freelancer1"))
                            .ReturnsAsync(expected);

            var result = await _controller.CheckChatExists("client1", "freelancer1");

            var value = Assert.IsType<ChatCheckResponseDTO>(result.Value);
            Assert.True(value.Exists);
            Assert.Equal(5, value.ChatId);
        }

        [Fact]
        public async Task CreateChat_ReturnsChatDTO()
        {
            var request = new CreateChatDto { ClientId = "client1", FreelancerId = "freelancer1" };
            var expected = new ChatDTO { ChatId = 1 };

            _chatServiceMock.Setup(s => s.CreateChatAsync(request))
                            .ReturnsAsync(expected);

            var result = await _controller.CreateChat(request);

            var value = Assert.IsType<ChatDTO>(result.Value);
            Assert.Equal(1, value.ChatId);
        }

        [Fact]
        public async Task CreateChat_ReturnsBadRequest_OnException()
        {
            var request = new CreateChatDto { ClientId = "client1", FreelancerId = "freelancer1" };

            _chatServiceMock.Setup(s => s.CreateChatAsync(request))
                            .ThrowsAsync(new Exception("Failed"));

            var result = await _controller.CreateChat(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Failed", badRequest.Value);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsMessages()
        {
            var expected = new List<MessageDTO> { new MessageDTO { MessageId = 1 } };

            _chatServiceMock.Setup(s => s.GetChatMessagesAsync(1, "user1"))
                            .ReturnsAsync(expected);

            var result = await _controller.GetChatMessages(1, "user1");

            var value = Assert.IsType<List<MessageDTO>>(result.Value);
            Assert.Single(value);
            Assert.Equal(1, value[0].MessageId);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsForbid_OnUnauthorized()
        {
            _chatServiceMock.Setup(s => s.GetChatMessagesAsync(1, "user1"))
                            .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.GetChatMessages(1, "user1");

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task SendMessage_ReturnsMessage()
        {
            var request = new SendMessageDTO { Content = "Hello", SenderId = "user1" };
            var expected = new MessageDTO { MessageId = 99 };

            _chatServiceMock.Setup(s => s.SendMessageAsync(1, request))
                            .ReturnsAsync(expected);

            var result = await _controller.SendMessage(1, request);

            var value = Assert.IsType<MessageDTO>(result.Value);
            Assert.Equal(99, value.MessageId);
        }

        [Fact]
        public async Task SendMessage_ReturnsForbid_OnUnauthorized()
        {
            var request = new SendMessageDTO { Content = "Hello", SenderId = "user1" };

            _chatServiceMock.Setup(s => s.SendMessageAsync(1, request))
                            .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.SendMessage(1, request);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetUserChats_ReturnsChats()
        {
            var expected = new List<ChatDTO> { new ChatDTO { ChatId = 88 } };

            _chatServiceMock.Setup(s => s.GetUserChatsAsync("user1"))
                            .ReturnsAsync(expected);

            var result = await _controller.GetUserChats("user1");

            var value = Assert.IsType<List<ChatDTO>>(result.Value);
            Assert.Single(value);
            Assert.Equal(88, value[0].ChatId);
        }
    }
}
