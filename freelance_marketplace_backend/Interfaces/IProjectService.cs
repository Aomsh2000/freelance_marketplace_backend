using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace freelance_marketplace_backend.Interfaces
{
    public interface IProjectService
    {
        // Project service interface
        // Interface to define the contract for Project services

        // Get project details by project ID
        Task<ProjectDetailsDto> GetProjectDetailsAsync(int projectId);
        Task<AssignProjectDto> AssignProjectToFreelancer(
            int projectId,
            AssignProjectDto model,
            string uid
        );
        Task<List<ProjectSummaryDto>> GetAllMyProjectsAsync(string freelancerId);
    }
}
