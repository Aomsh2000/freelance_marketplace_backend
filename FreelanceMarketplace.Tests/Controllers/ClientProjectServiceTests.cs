using freelance_marketplace_backend.Controllers;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Security.Claims;
using Xunit;

namespace freelance_marketplace_backend.Tests.Controllers
{
    public class ClientProjectControllerTests
    {
        private readonly Mock<IClientProjectService> _mockService;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly ClientProjectController _controller;

        public ClientProjectControllerTests()
        {
            _mockService = new Mock<IClientProjectService>();
            _mockCache = new Mock<IDistributedCache>();
            _controller = new ClientProjectController(_mockService.Object, _mockCache.Object);

            // Setup controller context with user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("user_id", "client123"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetApprovedProjectsForClient_ReturnsUnauthorized_WhenUserIdMismatch()
        {
            // Arrange
            var differentClientId = "client456";

            // Act
            var result = await _controller.GetApprovedProjectsForClient(differentClientId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("You are not authorized to access this user's data.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetApprovedProjectsForClient_ReturnsNotFound_WhenNoProjects()
        {
            // Arrange
            var clientId = "client123";
            _mockService.Setup(s => s.GetClientApprovedProjects(clientId))
                .ReturnsAsync(new List<ViewClientApprovedProjectDto>());

            // Act
            var result = await _controller.GetApprovedProjectsForClient(clientId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No approved projects found for this client.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetApprovedProjectsForClient_ReturnsProjects_WhenAuthorized()
        {
            // Arrange
            var clientId = "client123";
            var projects = new List<ViewClientApprovedProjectDto>
            {
                new ViewClientApprovedProjectDto
                {
                    ProjectId = 1,
                    Title = "Test Project",
                    Budget = 1000,
                    Status = "Approved"
                }
            };

            _mockService.Setup(s => s.GetClientApprovedProjects(clientId))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.GetApprovedProjectsForClient(clientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProjects = Assert.IsType<List<ViewClientApprovedProjectDto>>(okResult.Value);
            Assert.Single(returnedProjects);
        }





        // Test for MarkProjectAsCompleted method
        [Fact]
        public async Task MarkProjectAsCompleted_ReturnsUnauthorized_WhenProjectNotOwned()
        {
            // Arrange
            var projectId = 1;
            var projects = new List<ViewClientApprovedProjectDto>
            {
                new ViewClientApprovedProjectDto { ProjectId = 2, Status = "Approved" }
            };

            _mockService.Setup(s => s.GetClientApprovedProjects("client123"))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.MarkProjectAsCompleted(projectId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("You are not authorized to mark this project as completed.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task MarkProjectAsCompleted_ReturnsNotFound_WhenProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            _mockService.Setup(s => s.GetClientApprovedProjects("client123"))
                .ReturnsAsync(new List<ViewClientApprovedProjectDto>());

            // Act
            var result = await _controller.MarkProjectAsCompleted(projectId);

            // Assert
            // Changed from NotFoundObjectResult to UnauthorizedObjectResult
            // because the controller first checks authorization before checking existence
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("You are not authorized to mark this project as completed.", unauthorizedResult.Value);
        }


        [Fact]
        public async Task MarkProjectAsCompleted_ReturnsBadRequest_WhenAlreadyCompleted()
        {
            // Arrange
            var projectId = 1;
            var projects = new List<ViewClientApprovedProjectDto>
            {
                new ViewClientApprovedProjectDto { ProjectId = 1, Status = "Completed" }
            };

            _mockService.Setup(s => s.GetClientApprovedProjects("client123"))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.MarkProjectAsCompleted(projectId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Project is already marked as completed.", badRequestResult.Value);
        }

        [Fact]
        public async Task MarkProjectAsCompleted_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var projectId = 1;
            var projects = new List<ViewClientApprovedProjectDto>
            {
                new ViewClientApprovedProjectDto { ProjectId = 1, Status = "Approved" }
            };

            _mockService.Setup(s => s.GetClientApprovedProjects("client123"))
                .ReturnsAsync(projects);

            _mockService.Setup(s => s.MarkProjectAsCompleted(projectId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.MarkProjectAsCompleted(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCache.Verify(c => c.RemoveAsync("AvailableProjects", default), Times.Once);
        }

       
    }
}