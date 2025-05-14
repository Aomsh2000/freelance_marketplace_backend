using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using freelance_marketplace_backend.Controllers;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace freelance_marketplace_backend.Tests.Controllers
{
    public class ProjectsController_InMemoryTests
    {
        private readonly ProjectsController _controller;
        private readonly FreelancingPlatformContext _context;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<IDistributedCache> _cacheMock;

        public ProjectsController_InMemoryTests()
        {
            // Set up in-memory database
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<FreelancingPlatformContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _context = new FreelancingPlatformContext(options);

            // Create Skill
            _context.Skills.Add(
                new Skill
                {
                    SkillId = 1,
                    Skill1 = "C#",
                    Category = "Development"
                }
            );

            // Create User
            var user = new User
            {
                Usersid = "test-user-id",
                Name = "Test User",
                Email = "test@example.com"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Create Project
            _context.Projects.Add(
                new Project
                {
                    ProjectId = 1,
                    Title = "Test Project",
                    Overview = "Overview here",
                    RequiredTasks = "Do some things",
                    AdditionalNotes = "Be awesome",
                    PostedBy = user.Usersid,
                    PostedByNavigation = user,
                    Budget = 100,
                    Status = "Open",
                    Deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );
            _context.SaveChanges();

            // Link Project to Skill
            _context.ProjectSkills.Add(new ProjectSkill { ProjectId = 1, SkillId = 1 });
            _context.SaveChanges();

            // Setup mocks
            _projectServiceMock = new Mock<IProjectService>();
            _cacheMock = new Mock<IDistributedCache>();

            var projectRepo = new ProjectRepository(_context);
            _controller = new ProjectsController(
                projectRepo,
                _projectServiceMock.Object,
                _cacheMock.Object,
                null // No TwilioService
            );

            // Setup mock user
            var claimsUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("user_id", "test-user-id") }, "mock")
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsUser }
            };
        }

        [Fact]
        public async Task GetMyPostedProjects_ReturnsUserProjects()
        {
            // Act
            var result = await _controller.GetMyPostedProjects();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var projects = Assert.IsType<List<ProjectSummaryDto>>(okResult.Value);
            Assert.Single(projects);
            Assert.Equal("Test Project", projects[0].Title);
        }

        [Fact]
        public async Task PostNewProjectAsync_ValidInput_ReturnsOkWithProjectId()
        {
            // Arrange
            var projectDto = new CreateProjectDto
            {
                Title = "New Project",
                ProjectOverview = "Project overview",
                RequiredTasks = "Required tasks",
                AdditionalNotes = "Notes",
                Budget = 200,
                Deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Skills = new List<SkillDto> { new SkillDto { SkillId = 1 } }
            };

            // Act
            var result = await _controller.PostNewProjectAsync(projectDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value.GetType().GetProperty("ProjectId").GetValue(okResult.Value);
            Assert.IsType<int>(returnValue);
            Assert.Equal(2, returnValue); // Assuming it's the second project in the DB

            // Verify cache clear
            _cacheMock.Verify(c => c.RemoveAsync("AvailableProjects", default), Times.Once());
        }

        [Fact]
        public async Task PostNewProjectAsync_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var projectDto = new CreateProjectDto
            {
                Title = "", // Invalid
                ProjectOverview = "Project overview",
                RequiredTasks = "Required tasks",
                Budget = 200,
                Deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Skills = new List<SkillDto> { new SkillDto { SkillId = 1 } }
            };

            // Act
            var result = await _controller.PostNewProjectAsync(projectDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Title is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteProject_ValidProject_ReturnsOk()
        {
            // Act
            var result = await _controller.DeleteProject(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Project with ID 1 has been marked as deleted.", okResult.Value);

            // Verify cache clear
            _cacheMock.Verify(c => c.RemoveAsync("AvailableProjects", default), Times.Once());
            _cacheMock.Verify(c => c.RemoveAsync("project:1", default), Times.Once());
        }

        [Fact]
        public async Task DeleteProject_NonExistentProject_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteProject(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project with ID 999 not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task AssignProjectToFreelancer_Unauthorized_ReturnsForbid()
        {
            // Arrange
            var assignDto = new AssignProjectDto { FreelancerId = "freelancer-id" };
            _projectServiceMock
                .Setup(s => s.AssignProjectToFreelancer(1, assignDto, "test-user-id"))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            // Act
            var result = await _controller.AssignProjectToFreelancer(1, assignDto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetAllMyWorkingProjects_ValidFreelancerId_ReturnsOk()
        {
            // Arrange
            var projects = new List<ProjectSummaryDto>
            {
                new ProjectSummaryDto { ProjectId = 1, Title = "Test Project" }
            };
            _projectServiceMock
                .Setup(s => s.GetAllMyProjectsAsync("freelancer-id"))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.GetAllMyWorkingProjects("freelancer-id");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProjects = Assert.IsType<List<ProjectSummaryDto>>(okResult.Value);
            Assert.Single(returnedProjects);
            Assert.Equal("Test Project", returnedProjects[0].Title);
        }
    }
}