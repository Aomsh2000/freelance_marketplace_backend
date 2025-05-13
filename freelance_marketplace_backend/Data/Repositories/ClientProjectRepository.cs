using freelance_marketplace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace freelance_marketplace_backend.Data.Repositories
{
    public class ClientProjectRepository
    {

        private readonly FreelancingPlatformContext _context;
        private readonly IDistributedCache _cache;
        public ClientProjectRepository(FreelancingPlatformContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Project>> GetApprovedProjectsForClientAsync(string clientId)
        {
            return await _context.Projects
                .Where(p => p.PostedBy == clientId && p.FreelancerId != null).Include(p => p.Freelancer).ToListAsync();
        }

        public async Task<bool> MarkProjectAsCompleted(int projectId)
        {
            // Step1: mark the project as completed
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
            project.Status = "Completed";


            // Step2: increase freelancer balance
            var freelancer = await _context.Users
                .FirstOrDefaultAsync(u => u.Usersid == project.FreelancerId);
            freelancer.Balance+= project.Budget;

            _context.Projects.Update(project);
            _context.Users.Update(freelancer);
            await _context.SaveChangesAsync();
            // clear cash to user balance
            var usercacheKey = $"UserProfile_{project.FreelancerId}";
            await _cache.RemoveAsync(usercacheKey);
            return true;
        }

        public async Task<string?> GetClientUserIdByProjectIdAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            return project?.PostedBy;
        }


    }
}
