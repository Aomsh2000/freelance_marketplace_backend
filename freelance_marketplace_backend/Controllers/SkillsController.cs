using freelance_marketplace_backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly FreelancingPlatformContext _context;
        private readonly IDistributedCache _cache;

        public SkillsController(FreelancingPlatformContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetSkills()
        {
            try
            {
                // Define cache key
                string cacheKey = "skills_list";

                // Try to get data from cache
                string cachedSkills = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedSkills))
                {
                    // Deserialize cached data
                    var skillsFromCache = JsonSerializer.Deserialize<List<object>>(cachedSkills);
                    return Ok(skillsFromCache);
                }

                // If cache is empty, query the database
                var skills = await _context.Skills
                    .Select(skill => new
                    {
                        skill.SkillId,
                        Skill = skill.Skill1,
                        skill.Category
                    })
                    .ToListAsync();

                // Serialize the data for caching
                string serializedSkills = JsonSerializer.Serialize(skills);

                // Set cache options (e.g., expire after 10 minutes)
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };

                // Store in cache
                await _cache.SetStringAsync(cacheKey, serializedSkills, cacheOptions);

                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}