using freelance_marketplace_backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly FreelancingPlatformContext _context;

        // Inject FreelancingPlatformContext via constructor
        public SkillsController(FreelancingPlatformContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetSkills()
        {
            var skills = _context.Skills
                .Select(skill => new
                {
                    skill.SkillId,
                    Skill = skill.Skill1,
                    skill.Category
                })
                .ToList();

            return Ok(skills);
        }
    }
}