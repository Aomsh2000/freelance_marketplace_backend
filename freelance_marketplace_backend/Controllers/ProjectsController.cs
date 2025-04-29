
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace freelance_marketplace_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProjectsController : ControllerBase
	{
		private readonly IProjectService _projectService;
		private readonly IDistributedCache _cache;

		public ProjectsController(IProjectService projectService, IDistributedCache cache)
		{
			_projectService = projectService;
			_cache = cache;
		}

		// PUT: api/projects/{projectId}/assign
		[HttpPut("{projectId}/assign")]
		public async Task<IActionResult> AssignProjectToFreelancer(int projectId, [FromBody] AssignProjectDto model)
		{
			var result = await _projectService.AssignProjectToFreelancer(projectId, model);

			if (result == null)
			{
				return NotFound("Project or Proposal not found, or insufficient balance");
			}

			// Invalidate (remove) cache for the project after assignment
			var cacheKey = $"project:{projectId}";
			await _cache.RemoveAsync(cacheKey);

			return Ok(result); // return the updated project details
		}




	}
}
