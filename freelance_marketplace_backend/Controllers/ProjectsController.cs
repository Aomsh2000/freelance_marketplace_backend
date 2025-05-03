
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace freelance_marketplace_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ProjectsController : ControllerBase
	{
		private readonly IProjectService _projectService;
		private readonly IDistributedCache _cache;

		public ProjectsController(IProjectService projectService, IDistributedCache cache)
		{
			_projectService = projectService;
			_cache = cache;
		}


		[HttpPut("{projectId}/assign")]
		public async Task<IActionResult> AssignProjectToFreelancer(int projectId, [FromBody] AssignProjectDto model)
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
				return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
			}
		}

	}
}
