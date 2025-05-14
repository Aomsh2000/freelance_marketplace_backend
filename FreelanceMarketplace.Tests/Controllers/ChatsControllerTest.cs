using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using freelance_marketplace_backend.Controllers;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace freelance_marketplace_backend.Tests.Controllers
{
    public class ChatsControllerTests
    {
        private readonly Mock<IChatService> _chatServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly ChatsController _controller;

        public ChatsControllerTests()
        {
            _chatServiceMock = new Mock<IChatService>();
            _cacheMock = new Mock<IDistributedCache>();
            _controller = new ChatsController(_chatServiceMock.Object, _cacheMock.Object);
        }

        private void SetFakeUser(string userId)
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim("user_id", userId) }, "mock")
                    ),
                },
            };
        }

        [Fact]
        public async Task CheckChatExists_ReturnsExpectedResult()
        {
            SetFakeUser("client1");

            var expected = new ChatCheckResponseDTO { Exists = true, ChatId = 5 };

            _chatServiceMock
                .Setup(s => s.CheckChatExistsAsync("client1", "freelancer1"))
                .ReturnsAsync(expected);

            var result = await _controller.CheckChatExists("client1", "freelancer1");

            var value = Assert.IsType<ChatCheckResponseDTO>(result.Value);
            Assert.True(value.Exists);
            Assert.Equal(5, value.ChatId);
        }

        [Fact]
        public async Task CreateChat_ReturnsChatDTO()
        {
            SetFakeUser("client1");

            var expected = new ChatDTO { ChatId = 1 };

            _chatServiceMock
                .Setup(s => s.CreateChatAsync(It.IsAny<CreateChatDto>()))
                .ReturnsAsync(expected);

            var result = await _controller.CreateChat(
                new CreateChatDto { ClientId = "client1", FreelancerId = "freelancer1" }
            );

            var value = Assert.IsType<ChatDTO>(result.Value);
            Assert.Equal(1, value.ChatId);
        }

        [Fact]
        public async Task CreateChat_ReturnsBadRequest_OnException()
        {
            SetFakeUser("client1");

            _chatServiceMock
                .Setup(s => s.CreateChatAsync(It.IsAny<CreateChatDto>()))
                .ThrowsAsync(new Exception("Failed"));

            var result = await _controller.CreateChat(
                new CreateChatDto { ClientId = "client1", FreelancerId = "freelancer1" }
            );

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Failed", badRequest.Value);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsForbid_OnUnauthorized()
        {
            SetFakeUser("user1");

            _cacheMock
                .Setup(c => c.GetAsync(It.IsAny<string>(), default))
                .ReturnsAsync((byte[])null); // simulate cache miss

            _chatServiceMock
                .Setup(s => s.GetChatMessagesAsync(1, "user1"))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.GetChatMessages(1, "user1");

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsMessages()
        {
            SetFakeUser("user1");

            _cacheMock
                .Setup(c => c.GetAsync(It.IsAny<string>(), default))
                .ReturnsAsync((byte[])null); // simulate cache miss

            var expected = new List<MessageDTO> { new MessageDTO { MessageId = 1 } };

            _chatServiceMock.Setup(s => s.GetChatMessagesAsync(1, "user1")).ReturnsAsync(expected);

            var result = await _controller.GetChatMessages(1, "user1");

            var value = Assert.IsType<List<MessageDTO>>(result.Value);
            Assert.Single(value);
            Assert.Equal(1, value[0].MessageId);
        }

        [Fact]
        public async Task GetUserChats_ReturnsChats()
        {
            SetFakeUser("user1");

            _cacheMock
                .Setup(c => c.GetAsync(It.IsAny<string>(), default))
                .ReturnsAsync((byte[])null); 

            var expected = new List<ChatDTO> { new ChatDTO { ChatId = 88 } };

            _chatServiceMock.Setup(s => s.GetUserChatsAsync("user1")).ReturnsAsync(expected);

            var result = await _controller.GetUserChats("user1");

            var value = Assert.IsType<List<ChatDTO>>(result.Value);
            Assert.Single(value);
            Assert.Equal(88, value[0].ChatId);
        }
    }
}
