using freelance_marketplace_backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientProjectController : ControllerBase
    {
        private readonly IClientProjectService _clientProjectService;

        public ClientProjectController(IClientProjectService clientProjectService)
        {
            _clientProjectService = clientProjectService;
        }

        [HttpGet("approved/{clientId}")]
        public async Task<IActionResult> GetApprovedProjectsForClient(string clientId)
        {
            // step 1: Check if the user has the this clientId
            var uid = User.FindFirst("user_id")?.Value;
            if (uid != clientId)
                return Unauthorized("You are not authorized to access this user's data.");

            // step 2: Get the approved projects for the client
            var projects = await _clientProjectService.GetClientApprovedProjects(clientId);
            if (projects == null || !projects.Any())
                return NotFound("No approved projects found for this client.");

            return Ok(projects);
        }



        [HttpPut("approved/{projectId}/mark-completed")]
        public async Task<IActionResult> MarkProjectAsCompleted(int projectId)
        {

            // step 1: Check if the user has the this projectId
            var uid = User.FindFirst("user_id")?.Value;
            var projects = await _clientProjectService.GetClientApprovedProjects(uid);
            if (projects == null || !projects.Any(p => p.ProjectId == projectId))
                return Unauthorized("You are not authorized to mark this project as completed.");
           
            // step 2: Check if the project is already completed
            var project = projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null)
                return NotFound("Project not found.");
            if (project.Status == "Completed")
                return BadRequest("Project is already marked as completed.");
           
            // step 3: Mark the project as completed
            var result = await _clientProjectService.MarkProjectAsCompleted(projectId);
            if (result)
            {
                return Ok(new { message = "Project marked as completed successfully." });
            }
            return BadRequest(new { message = "Failed to mark the project as completed." });
        }


    }
}
