using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using freelance_marketplace_backend.Controllers;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Xunit;
using System.Text.Json;
using System.Threading;
using System.Text;

namespace freelance_marketplace_backend.Tests.Controllers
{
    public class FreelancerProposalControllerTests
    {
        private readonly Mock<IProjectService> _mockProjectService;
        private readonly Mock<IProposalService> _mockProposalService;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly FreelancerProposalController _controller;

        public FreelancerProposalControllerTests()
        {
            _mockProjectService = new Mock<IProjectService>();
            _mockProposalService = new Mock<IProposalService>();
            _mockCache = new Mock<IDistributedCache>();
            _controller = new FreelancerProposalController(
                _mockProjectService.Object,
                _mockProposalService.Object,
                _mockCache.Object
            );
        }

        [Fact]
        public async Task GetProjectById_ReturnsOk_WhenProjectIsFoundInCache()
        {
            // Arrange
            int projectId = 1;
            var projectDetails = new ProjectDetailsDto
            {
                ProjectId = projectId,
                Title = "Test Project"
            };
            var cachedProject = JsonSerializer.Serialize(projectDetails);
            
            _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(cachedProject));

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProject = Assert.IsType<ProjectDetailsDto>(okResult.Value);
            Assert.Equal(projectId, returnedProject.ProjectId);
        }

        [Fact]
        public async Task GetProjectById_ReturnsNotFound_WhenProjectNotFoundInCacheOrService()
        {
            // Arrange
            int projectId = 999;
            
            _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);
                
            // استخدم ReturnsAsync مع Task.FromResult لتجنب التحذير
            _mockProjectService.Setup(s => s.GetProjectDetailsAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<ProjectDetailsDto?>(null));

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // باقي الاختبارات تبقى كما هي بدون تغيير
        [Fact]
        public async Task SubmitProposal_ReturnsCreated_WhenProposalIsValid()
        {
            // Arrange
            int projectId = 1;
            var proposalDto = new CreateProposalDto
            {
                FreelancerId = "freelancer1",
                ProposedAmount = 500,
                Deadline = DateTime.Now.AddDays(7),
                CoverLetter = "Proposal cover letter"
            };
            var proposal = new ProposalDto
            {
                ProposalId = 1,
                ProjectId = projectId,
                FreelancerId = "freelancer1",
                FreelancerName = "Freelancer Name",
                ProposedAmount = 500,
                Deadline = DateOnly.FromDateTime(proposalDto.Deadline),
                CoverLetter = proposalDto.CoverLetter,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _mockProposalService.Setup(s => s.SubmitProposalAsync(projectId, proposalDto))
                .ReturnsAsync(proposal);

            // Act
            var result = await _controller.SubmitProposal(projectId, proposalDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("SubmitProposal", createdResult.ActionName);
            var returnedProposal = Assert.IsType<ProposalDto>(createdResult.Value);
            Assert.Equal(proposal.ProposalId, returnedProposal.ProposalId);
        }

        [Fact]
        public async Task SubmitProposal_ReturnsNotFound_WhenProjectNotFound()
        {
            // Arrange
            int projectId = 999;
            var proposalDto = new CreateProposalDto
            {
                FreelancerId = "freelancer1",
                ProposedAmount = 500,
                Deadline = DateTime.Now.AddDays(7),
                CoverLetter = "Proposal cover letter"
            };
            _mockProposalService.Setup(s => s.SubmitProposalAsync(projectId, proposalDto))
                .ThrowsAsync(new KeyNotFoundException("Project not found."));

            // Act
            var result = await _controller.SubmitProposal(projectId, proposalDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetMyProposals_ReturnsOk_WhenProposalsExist()
        {
            // Arrange
            string freelancerId = "freelancer1";
            var proposals = new List<ProposalDto>
            {
                new ProposalDto
                {
                    ProposalId = 1,
                    ProjectId = 1,
                    FreelancerId = freelancerId,
                    FreelancerName = "Freelancer Name",
                    ProposedAmount = 500,
                    Deadline = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                    CoverLetter = "Proposal cover letter",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                }
            };
            _mockProposalService.Setup(s => s.GetProposalsByFreelancerIdAsync(freelancerId))
                .ReturnsAsync(proposals);

            // Act
            var result = await _controller.GetMyProposals(freelancerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultList = Assert.IsType<List<ProposalDto>>(okResult.Value);
            Assert.Single(resultList);
        }

        [Fact]
        public async Task DeleteProposal_ReturnsOk_WhenProposalIsDeleted()
        {
            // Arrange
            int proposalId = 1;
            string freelancerId = "freelancer1";
            _mockProposalService.Setup(s => s.DeleteProposalAsync(proposalId, freelancerId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProposal(proposalId, freelancerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteProposal_ReturnsNotFound_WhenProposalNotFound()
        {
            // Arrange
            int proposalId = 999;
            string freelancerId = "freelancer1";
            _mockProposalService.Setup(s => s.DeleteProposalAsync(proposalId, freelancerId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProposal(proposalId, freelancerId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}