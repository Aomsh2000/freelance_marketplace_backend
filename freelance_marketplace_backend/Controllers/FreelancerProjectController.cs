using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Models.Dtos;
using AdvancedAjax.Models.Dtos;

namespace freelance_marketplace_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FreelancerProjectController : ControllerBase
    {
        private readonly FreelancingPlatformContext _context;
        private readonly IDistributedCache _cache;

        public FreelancerProjectController(FreelancingPlatformContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("all/available/projects")]
        public async Task<ActionResult<IEnumerable<ProfileProjectDto>>> GetAllAvailableProjects()
        {
            var cacheKey = "AvailableProjects";
            var cachedProjects = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProjects))
            {
                var projectsFromCache = JsonSerializer.Deserialize<List<ProfileProjectDto>>(cachedProjects);
                return Ok(projectsFromCache);
            }

            var projects = await _context.Projects
                .Where(p => p.Status == "Open" && p.FreelancerId == null && p.IsDeleted!=true)
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .ToListAsync();

            var projectDtos = projects.Select(p => new ProfileProjectDto
            {
                ProjectId = p.ProjectId,
                Title = p.Title,
                ProjectOverview = p.Overview,
                RequiredTasks = p.RequiredTasks,
                AdditionalNotes = p.AdditionalNotes,
                Budget = p.Budget,
                Deadline = p.Deadline,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                Skills = p.ProjectSkills.Select(ps => new SkillDto
                {
                    SkillId = ps.Skill.SkillId,
                    Skill = ps.Skill.Skill1,
                    Category = ps.Skill.Category
                }).ToList()

            }).ToList();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            };

            var serializedProjects = JsonSerializer.Serialize(projectDtos);
            await _cache.SetStringAsync(cacheKey, serializedProjects, cacheOptions);

            return Ok(projectDtos);
        }
    }
}
