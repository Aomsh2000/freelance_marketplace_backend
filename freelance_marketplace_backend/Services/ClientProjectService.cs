using System.Text.Json;
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace freelance_marketplace_backend.Services
{
    public class ClientProjectService : IClientProjectService
    {

        private readonly ClientProjectRepository _clientProjectRepository;
        private readonly IDistributedCache _cache;

        // Service use the repository to access the data
        public ClientProjectService(ClientProjectRepository clientProjectRepository, IDistributedCache cache)
        {
            _clientProjectRepository = clientProjectRepository;
            _cache = cache;
        }

        public async Task<List<ViewClientApprovedProjectDto>> GetClientApprovedProjects(string userID)
        {
            //Caching
            string cacheKey = $"approved_projects_{userID}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<ViewClientApprovedProjectDto>>(cachedData)!;
            }

            // Get the approved projects for the client using the repository function
            var approvedProjects = await _clientProjectRepository.GetApprovedProjectsForClientAsync(userID);


            // Map the projects to the DTO
            var projectDtos = approvedProjects.Select(p => new ViewClientApprovedProjectDto
            {
                ProjectId = p.ProjectId,
                Title = p.Title,
                Budget = p.Budget,
                Deadline = p.Deadline,
                Status = p.Status,
                Freelancer = new FreelancerDto
                {
                    FreelancerId = p.FreelancerId,
                    FreelancerName = p.Freelancer.Name
                }
            }).ToList();

            if (projectDtos.Any())
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                };

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(projectDtos), options);
            }
            return projectDtos;
        }

        public async Task<bool> MarkProjectAsCompleted(int projectId)
        {
            var success = await _clientProjectRepository.MarkProjectAsCompleted(projectId);
            if (success)
            {
                // Get the user ID of the client who owns this project
                var userId = await _clientProjectRepository.GetClientUserIdByProjectIdAsync(projectId);
                if (!string.IsNullOrEmpty(userId))
                {
                    string cacheKey = $"approved_projects_{userId}";
                    await _cache.RemoveAsync(cacheKey); // Clear cache for this user

                   
                }
            }
            return success;
        }



    }
}
