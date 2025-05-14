using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using freelance_marketplace_backend.Controllers;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using freelance_marketplace_backend.Services;
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
        private ProjectsController _controller;
        private FreelancingPlatformContext _context;

        public ProjectsController_InMemoryTests()
        {
            // إعداد مزود خدمة داخلي خاص بـ InMemory
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<FreelancingPlatformContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _context = new FreelancingPlatformContext(options);

            // إنشاء Skill
            _context.Skills.Add(
                new Skill
                {
                    SkillId = 1,
                    Skill1 = "C#",
                    Category = "Development",
                }
            );

            // إنشاء مستخدم
            var user = new User
            {
                Usersid = "test-user-id",
                Name = "test-user-id",
                Email = "test@example.com",
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // إنشاء المشروع وربطه بالمستخدم
            _context.Projects.Add(
                new Project
                {
                    ProjectId = 1,
                    Title = "Test Project",
                    Overview = "Overview here",
                    RequiredTasks = "Do some things",
                    AdditionalNotes = "Be awesome",
                    PostedBy = user.Usersid,
                    PostedByNavigation = user, // 🔥 هذا الربط هو المهم
                    Budget = 100,
                    Status = "Open",
                    Deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                }
            );
            _context.SaveChanges();

            // ربط المشروع بـ Skill
            _context.ProjectSkills.Add(new ProjectSkill { ProjectId = 1, SkillId = 1 });
            _context.SaveChanges();

            var projectRepo = new ProjectRepository(_context);
            var projectServiceMock = new Mock<Interfaces.IProjectService>();
            var cacheMock = new Mock<IDistributedCache>();

            _controller = new ProjectsController(
                projectRepo,
                projectServiceMock.Object,
                cacheMock.Object,
                null // لا حاجة لـ Twilio في هذا الاختبار
            );

            // إعداد مستخدم وهمي للـ HttpContext
            var claimsUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("user_id", "test-user-id") }, "mock")
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsUser },
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
    }
}
