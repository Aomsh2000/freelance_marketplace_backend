using System.Threading;
using System.Threading.Tasks;
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using freelance_marketplace_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;



namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
 
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IDistributedCache _cache;

        //private readonly TwilioService _twilioService;
        private readonly ProjectRepository _projectRepository;

        public ProjectsController(
            ProjectRepository projectRepository,
            IProjectService projectService,
            IDistributedCache cache
            //TwilioService twilioService

        )
		{
            _projectRepository = projectRepository;
            _projectService = projectService;
            _cache = cache;
            //_twilioService = twilioService;

        }

		// GET: api/projects/mine
		[HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMyPostedProjects()
        {
            var uid = User.FindFirst("user_id")?.Value;
            if (uid == null)
                return Unauthorized("User ID not found in token.");

            var myProjects = await _projectRepository.GetProjectsByUserAsync(uid);
            return Ok(myProjects);
        }

        // POST: api/projects/create
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> PostNewProjectAsync(
            [FromBody] CreateProjectDto project,
            CancellationToken cancellationToken = default
        )
        {
            var uid = User.FindFirst("user_id")?.Value;
            if (uid == null)
                return Unauthorized("User ID not found in token.");

            if (string.IsNullOrWhiteSpace(project.Title))
                return BadRequest("Title is required.");
            if (string.IsNullOrWhiteSpace(project.ProjectOverview))
                return BadRequest("Project overview is required.");
            if (string.IsNullOrWhiteSpace(project.RequiredTasks))
                return BadRequest("Required tasks are required.");
            if (project.Budget <= 0)
                return BadRequest("Budget must be greater than zero.");
            if (project.Deadline == default)
                return BadRequest("A valid deadline is required.");

            var newProject = new Project
            {
                Title = project.Title,
                Overview = project.ProjectOverview,
                RequiredTasks = project.RequiredTasks,
                AdditionalNotes = project.AdditionalNotes ?? "",
                Budget = project.Budget,
                Deadline = project.Deadline,
                PostedBy = uid,
                Status = "Open",
            };

            await _projectRepository.AddProjectAsync(newProject, project.Skills, cancellationToken);
            return Ok(new { newProject.ProjectId });
        }

        // DELETE: api/projects/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProject(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            var uid = User.FindFirst("user_id")?.Value;
            if (uid == null)
                return Unauthorized("User ID not found in token.");

            var result = await _projectRepository.MarkProjectAsDeletedAsync(
                id,
                uid,
                cancellationToken
            );
            if (result == "NotFound")
                return NotFound($"Project with ID {id} not found.");
            if (result == "Unauthorized")
                return Unauthorized("You only can delete your projects");

            return Ok($"Project with ID {id} has been marked as deleted.");
        }

        // PUT: api/projects/{projectsid}/assign

        [HttpPut("{projectId}/assign")]
        [Authorize]
        public async Task<IActionResult> AssignProjectToFreelancer( int projectId, [FromBody] AssignProjectDto model)
        {
            try
            {
                //extract userid from token
                var uid = User.FindFirst("user_id")?.Value;

                if (uid == null)
                {
                    return Unauthorized("Unauthorized: user_id is missing in the token.");
                }

                // Request the assignment from the service
                var result = await _projectService.AssignProjectToFreelancer(projectId, model, uid);


                if (result == null)
                {
                    return NotFound("Project or proposal not found.");
                }

				//var message = $"Congratulations! Your Proposal has been accepted for the project Number: {projectId}";

				//await _twilioService.SendSmsAsync(model.FreelancerPhoneNumber, message);

				// Invalidate the cache for the project after assignment
				await _cache.RemoveAsync($"project:{projectId}");
			
				// Return the updated project details
				return Ok(result);

		

			}
			catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            // Handle any InvalidOperationException (e.g., trying to assign a freelancer to a project already assigned)
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            // Handle any other unexpected errors
            catch (Exception ex)
            {
                return StatusCode( 500,new
                    {
                        message = "An error occurred while processing your request.",
                        error = ex.Message,
                    }
                );
            }
        }

        [HttpGet("Get-AllMyWorkingProjects/{freelancerId}")]
        public async Task<IActionResult> GetAllMyWorkingProjects(string freelancerId)
        {
            var result = await _projectService.GetAllMyProjectsAsync(freelancerId);
            return Ok(result);
        }
    }
}
