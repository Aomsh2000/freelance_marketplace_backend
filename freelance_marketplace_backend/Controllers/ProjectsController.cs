using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using freelance_marketplace_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectRepository _projectRepository;
        private readonly IProjectService _projectService;
        private readonly IDistributedCache _cache;

        public ProjectsController(
            ProjectRepository projectRepository,
            IProjectService projectService,
            IDistributedCache cache)
        {
            _projectRepository = projectRepository;
            _projectService = projectService;
            _cache = cache;
        }

        // GET: api/projects/mine
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMyPostedProjects()
        {
            var uid = User.FindFirst("user_id")?.Value;
            if (uid == null) return Unauthorized("User ID not found in token.");

            var myProjects = await _projectRepository.GetProjectsByUserAsync(uid);
            return Ok(myProjects);
        }

        // POST: api/projects/create
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> PostNewProjectAsync([FromBody] CreateProjectDto project, CancellationToken cancellationToken = default)
        {
            var uid = User.FindFirst("user_id")?.Value;
            if (uid == null) return Unauthorized("User ID not found in token.");

            if (string.IsNullOrWhiteSpace(project.Title)) return BadRequest("Title is required.");
            if (string.IsNullOrWhiteSpace(project.ProjectOverview)) return BadRequest("Project overview is required.");
            if (string.IsNullOrWhiteSpace(project.RequiredTasks)) return BadRequest("Required tasks are required.");
            if (project.Budget <= 0) return BadRequest("Budget must be greater than zero.");
            if (project.Deadline == default) return BadRequest("A valid deadline is required.");

            var newProject = new Project
            {
                Title = project.Title,
                Overview = project.ProjectOverview,
                RequiredTasks = project.RequiredTasks,
                AdditionalNotes = project.AdditionalNotes ?? "",
                Budget = project.Budget,
                Deadline = project.Deadline,
                PostedBy = uid,
                Status = "Open"
            };

            await _projectRepository.AddProjectAsync(newProject, project.Skills, cancellationToken);
            return Ok(new { newProject.ProjectId });
        }

        // DELETE: api/projects/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProject(int id, CancellationToken cancellationToken = default)
        {
            var uid = User.FindFirst("user_id")?.Value;
            if (uid == null) return Unauthorized("User ID not found in token.");

            var result = await _projectRepository.MarkProjectAsDeletedAsync(id, uid, cancellationToken);
            if (result == "NotFound") return NotFound($"Project with ID {id} not found.");
            if (result == "Unauthorized") return Unauthorized("You only can delete your projects");

            return Ok($"Project with ID {id} has been marked as deleted.");
        }

        //// GET: api/projects/{id}
        //[HttpGet("{id:int}")]
        //[Authorize]
        //public async Task<IActionResult> GetProjectById(int id)
        //{
        //    var project = await _projectRepository.GetProjectByIdAsync(id);
        //    if (project == null)
        //        return NotFound($"Project with ID {id} not found.");

        //    return Ok(project);
        //}

        // PUT: api/projects/{projectId}/assign
        [HttpPut("{projectId}/assign")]
        public async Task<IActionResult> AssignProjectToFreelancer(int projectId, [FromBody] AssignProjectDto model)
        {
            var result = await _projectService.AssignProjectToFreelancer(projectId, model);

            if (result == null)
                return NotFound("Project or Proposal not found, or insufficient balance");

            // Invalidate cache
            var cacheKey = $"project:{projectId}";
            await _cache.RemoveAsync(cacheKey);

            return Ok(result);
        }
    }
}
