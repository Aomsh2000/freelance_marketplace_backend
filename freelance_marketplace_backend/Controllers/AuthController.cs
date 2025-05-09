using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Models.Dtos;
using AdvancedAjax.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace freelance_marketplace_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FreelancingPlatformContext _context;
        private readonly IDistributedCache _cache;

        public AuthController(FreelancingPlatformContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<UserProfileDto>> GetUserById(string userId)
        {
            var cacheKey = $"UserProfile_{userId}";
            var cachedUser = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedUser))
            {
                var userProfileDto = JsonSerializer.Deserialize<UserProfileDto>(cachedUser);
                return Ok(userProfileDto);
            }

            var user = await _context.Users
                .Where(u => u.Usersid == userId && (u.IsDeleted == null || u.IsDeleted == false))
                .Include(u => u.UsersSkills).ThenInclude(us => us.Skill)
                .Include(u => u.ProjectFreelancers)
                .ThenInclude(p => p.ProjectSkills)
                .ThenInclude(ps => ps.Skill)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var projects = await _context.Projects
            .Where(p => p.FreelancerId == userId && p.Status == "Completed" )
            .Include(p => p.ProjectSkills)
            .ThenInclude(ps => ps.Skill)
            .ToListAsync();


            var userProfile = new UserProfileDto
            {
                UserId = user.Usersid,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                ImageUrl = user.ImageUrl,
                AboutMe = user.AboutMe,
                Rating = user.Rating,
                Balance = user.Balance,
                Skills = user.UsersSkills.Select(us => new SkillDto
                {
                    SkillId = us.Skill.SkillId,
                    Skill = us.Skill.Skill1,
                    Category = us.Skill.Category
                }).ToList(),
                Projects = projects.Select(p => new ProfileProjectDto
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
                }).ToList()
            };

            // Save to Redis cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) //
            };

            var serializedUserProfile = JsonSerializer.Serialize(userProfile);
            await _cache.SetStringAsync(cacheKey, serializedUserProfile, cacheOptions);

            return Ok(userProfile);
        }

        [Authorize]
        [HttpPut("balance/change")]
        public async Task<IActionResult> ChangeBalance([FromBody] BalanceChangeDto request)
        {
            if (request == null)
                return BadRequest("Request body is missing.");

            //Get userID from token and check if it exists
            var uid = User.FindFirst("user_id")?.Value;
            if (uid == null)
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Usersid == uid);
            if (user == null)
                return NotFound("User not found.");

            // increment balance
            user.Balance += request.Amount;
            await _context.SaveChangesAsync();

            // clear cash
            var cacheKey = $"UserProfile_{uid}";
            await _cache.RemoveAsync(cacheKey);

            return Ok(new { message = "Balance updated successfully.", newBalance = user.Balance });
        }
    }
}
